using Microsoft.Extensions.Logging;
using ScreenTemperature.Entities;
using System.Drawing;
using System.Runtime.InteropServices;
using WindowsDisplayAPI;
using static ScreenTemperature.Win32;

namespace ScreenTemperature.Services;

public interface IScreenService
{
    Task<IList<Screen>> GetScreensAsync();
    Task<bool> ApplyKelvinToScreenAsync(int value, string devicePath);
    Task<bool> ApplyColorToScreenAsync(string stringColor, string devicePath);
    Task<bool> ApplyBrightnessToScreenAsync(int brightness, string devicePath);
}

public class ScreenService(ILogger<ScreenService> logger) : IScreenService
{
    private bool GetScreenPhysicalMonitor(string devicePath, out PHYSICAL_MONITOR[] physicalMonitors)
    {
        physicalMonitors = [];

        var display = WindowsDisplayAPI.Display.GetDisplays()?.FirstOrDefault(x => x.DevicePath == devicePath);

        if (display == null) return false;

        var monitorsHandle = new List<IntPtr>();

        if (!EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
        delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
        {
            monitorsHandle.Add(hMonitor);
            return true;
        }, IntPtr.Zero)) return false;

        var displayMonitorHandle = IntPtr.Zero;

        foreach (var monitorHandle in monitorsHandle)
        {
            var monitorInfo = new MonitorInfo
            {
                Size = (uint)Marshal.SizeOf(typeof(MonitorInfo))
            };

            if (!GetMonitorInfo(monitorHandle, ref monitorInfo)) continue;

            if (monitorInfo.DisplayName == display.DisplayName)// if this monitor is the one of the display we are looking for
            {
                displayMonitorHandle = monitorHandle;
                break;
            }
        }

        if (displayMonitorHandle == IntPtr.Zero) return false;

        if (!GetNumberOfPhysicalMonitorsFromHMONITOR(displayMonitorHandle, out uint numberOfPhysicalMonitors)) return false;

        if (numberOfPhysicalMonitors == 0 || numberOfPhysicalMonitors > 1) return false;

        physicalMonitors = new PHYSICAL_MONITOR[numberOfPhysicalMonitors];

        if (!GetPhysicalMonitorsFromHMONITOR(displayMonitorHandle, numberOfPhysicalMonitors, physicalMonitors)) return false;

        return true;
    }

    private IList<Screen> _screens;
    private Task _screensLoadTask;

    /// <summary>
    /// Returns a list of all attached screens on this machine
    /// </summary>
    public async Task<IList<Screen>> GetScreensAsync()
    {
        if (_screensLoadTask != null)
        {
            await _screensLoadTask;

            foreach (var screen in _screens)// for each screen, refresh its physical monitor handle (they can change for example when the screen goes to sleep mode)
            {
                if (!screen.IsDDCSupported) continue;

                DestroyPhysicalMonitors(1, screen.PhysicalMonitors);// delete previous handles
                var physicalMonitor = GetScreenPhysicalMonitor(screen.DevicePath, out var physicalMonitors);// get new ones

                screen.PhysicalMonitors = physicalMonitors;
            }

            return _screens;
        }

        _screensLoadTask = new Task(() => { });
        _screens = new List<Screen>();

        foreach (var display in Display.GetDisplays())// for each windows display
        {
            if(!GetScreenPhysicalMonitor(display.DevicePath, out PHYSICAL_MONITOR[] physicalMonitors)) continue;

            uint min = 0, max = 0, current = 0;
            bool isDDCsupported = false;
            bool isBrightnessSupported = false;

            isDDCsupported = GetMonitorCapabilities(physicalMonitors[0].hPhysicalMonitor, out MC_CAPS pdwMonitorCapabilities, out MC_SUPPORTED_COLOR_TEMPERATURE pdwSupportedColorTemperatures);

            if (isDDCsupported)
            {
                isBrightnessSupported = pdwMonitorCapabilities.HasFlag(MC_CAPS.MC_CAPS_BRIGHTNESS);
            }

            if (isBrightnessSupported)
                GetMonitorBrightness(physicalMonitors[0].hPhysicalMonitor, ref min, ref current, ref max);

            _screens.Add(new Screen()
            {
                Label = display.ToPathDisplayTarget().FriendlyName,
                Width = display.CurrentSetting.Resolution.Width,
                Height = display.CurrentSetting.Resolution.Height,
                IsPrimary = display.IsGDIPrimary,
                X = display.CurrentSetting.Position.X,
                Y = display.CurrentSetting.Position.Y,
                DevicePath = display.DevicePath,
                IsDDCSupported = isDDCsupported,
                IsBrightnessSupported = isBrightnessSupported,
                MinBrightness = min,
                MaxBrightness = max,
                CurrentBrightness = current,
                PhysicalMonitors = physicalMonitors,
            });
        }

        _screensLoadTask.Start();

        return _screens;
    }

    private bool ApplyRGBToScreen(float red, float green, float blue, string devicePath)
    {
        var array = new ushort[3 * 256];

        for (var ik = 0; ik < 256; ik++)
        {
            array[ik] = (ushort)(ik * red);
            array[256 + ik] = (ushort)(ik * green);
            array[512 + ik] = (ushort)(ik * blue);
        }

        var display = WindowsDisplayAPI.Display.GetDisplays().FirstOrDefault(x => x.DevicePath == devicePath);

        if (display == null) throw new Exception($"Could not find screen '{devicePath}'.");

        var hdc = CreateDC(display.DisplayName, display.DevicePath, null, IntPtr.Zero);

        var pinnedArray = GCHandle.Alloc(array, GCHandleType.Pinned);
        IntPtr pointer = pinnedArray.AddrOfPinnedObject();

        var succeeded = SetDeviceGammaRamp(hdc.ToInt32(), pointer);

        DeleteDC(hdc);

        pinnedArray.Free();

        return succeeded;
    }

    /// <summary>
    /// Changes screen color from kelvin value
    /// Thanks to Tanner Helland for his algorithm http://www.tannerhelland.com/4435/convert-temperature-rgb-algorithm-code/
    /// </summary>
    public async Task<bool> ApplyKelvinToScreenAsync(int value, string devicePath)
    {
        if(value < 1000 || value > 40000)
        {
            throw new Exception("Invalid value.");
        }

        float kelvin = value;
        var temperature = kelvin / 100;

        float red, green, blue;

        if (temperature <= 66)
        {
            red = 255;
        }
        else
        {
            red = temperature - 60;
            red = 329.698727446f * ((float)Math.Pow(red, -0.1332047592));

            if (red < 0) red = 0;
            if (red > 255) red = 255;
        }

        if (temperature <= 66)
        {
            green = temperature;
            green = 99.4708025861f * (float)Math.Log(green) - 161.1195681661f;

            if (green < 0) green = 0;
            if (green > 255) green = 255;
        }
        else
        {
            green = temperature - 60;
            green = 288.1221695283f * ((float)Math.Pow(green, -0.0755148492));

            if (green < 0) green = 0;
            if (green > 255) green = 255;
        }


        if (temperature >= 66)
        {
            blue = 255;
        }
        else
        {
            if (temperature <= 19)
            {
                blue = 0;
            }
            else
            {
                blue = temperature - 10;
                blue = 138.5177312231f * (float)Math.Log(blue) - 305.0447927307f;
                if (blue < 0) blue = 0;
                if (blue > 255) blue = 255;
            }
        }

        if (value == 6600)
        {
            red = 255;
            green = 255;
            blue = 255;
        }

        return ApplyRGBToScreen(red, green, blue, devicePath);
    }

    public async Task<bool> ApplyColorToScreenAsync(string stringColor, string devicePath)
    {
        ColorConverter converter = new ColorConverter();
        var color = (Color?)converter.ConvertFromString(stringColor);

        if (!color.HasValue)
        {
            throw new Exception("Invalid value.");
        }

        return ApplyRGBToScreen(color.Value.R, color.Value.G, color.Value.B, devicePath);
    }

    public async Task<bool> ApplyBrightnessToScreenAsync(int brightness, string devicePath)
    {
        var screens = await GetScreensAsync();
        var screen = screens.FirstOrDefault(x => x.DevicePath ==  devicePath);

        if (screen == null) throw new Exception($"Could not find screen '{devicePath}'.");

        var succeeded = false;

        // calculate maximum when minimum is 0
        var maximum = screen.MaxBrightness - screen.MinBrightness;

        // brightness is in percentage
        var valueToApply = (uint)brightness * maximum / 100 + screen.MinBrightness;

        succeeded = SetMonitorBrightness(screen.PhysicalMonitors[0].hPhysicalMonitor, valueToApply);

        return succeeded;
    }
}