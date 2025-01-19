
using ScreenTemperature.DTOs;
using ScreenTemperature.Entities;
using ScreenTemperature.Mappers;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ScreenTemperature.Services;

public interface IScreenService
{
    /// <summary>
    /// Returns a list of all attached screens on this machine
    /// </summary>
    ServiceResult<IList<ScreenDto>> GetScreens();
    ServiceResult<bool> ApplyKelvinToScreen(int value, string devicePath);
    ServiceResult<bool> ApplyColorToScreen(string stringColor, string devicePath);
    ServiceResult<bool> ApplyBrightnessToScreen(int brightness, string devicePath);
}

public class ScreenService : IScreenService
{
    private readonly ILogger<ScreenService> _logger;

    public ScreenService(ILogger<ScreenService> logger)
    {
        _logger = logger;
    }

    [DllImport("gdi32.dll")]
    private static extern bool SetDeviceGammaRamp(int hdc, IntPtr ramp);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateDC(string lpszDriver, string? lpszDevice, string? lpszOutput, IntPtr lpInitData);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hdc);

    [DllImport("Dxva2.dll")]
    private static extern bool GetMonitorBrightness(IntPtr hdc, ref uint pdwMinimumBrightness, ref uint pdwCurrentBrightness, ref uint pdwMaximumBrightness);

    [DllImport("Dxva2.dll")]
    private static extern bool SetMonitorBrightness(IntPtr hMonitor, uint dwNewBrightness);

    [DllImport("Dxva2.dll")]
    private static extern bool GetMonitorCapabilities(IntPtr hdc, out MC_CAPS pdwMonitorCapabilities, out MC_SUPPORTED_COLOR_TEMPERATURE pdwSupportedColorTemperatures);

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);

    delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

    [Flags]
    internal enum MonitorInfoFlags : uint
    {
        None = 0,
        Primary = 1
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo
    {
        internal uint Size;
        public readonly Rect Bounds;
        public readonly Rect WorkingArea;
        public readonly MonitorInfoFlags Flags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public readonly string DisplayName;
    }

    [DllImport("user32")]
    private static extern bool GetMonitorInfo(IntPtr monitorHandle, ref MonitorInfo monitorInfo);

    [DllImport("Dxva2.dll")]
    private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, out uint pdwNumberOfPhysicalMonitors);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct PHYSICAL_MONITOR
    {
        public IntPtr hPhysicalMonitor;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szPhysicalMonitorDescription;
    }

    [DllImport("Dxva2.dll")]
    private static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

    [DllImport("Dxva2.dll")]
    private static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, [In] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

    // found on https://github.com/emoacht/Monitorian/blob/master/Source/Monitorian.Core/Models/Monitor/MonitorConfiguration.cs
    [Flags]
    private enum MC_CAPS
    {
        MC_CAPS_NONE = 0x00000000,
        MC_CAPS_MONITOR_TECHNOLOGY_TYPE = 0x00000001,
        MC_CAPS_BRIGHTNESS = 0x00000002,
        MC_CAPS_CONTRAST = 0x00000004,
        MC_CAPS_COLOR_TEMPERATURE = 0x00000008,
        MC_CAPS_RED_GREEN_BLUE_GAIN = 0x00000010,
        MC_CAPS_RED_GREEN_BLUE_DRIVE = 0x00000020,
        MC_CAPS_DEGAUSS = 0x00000040,
        MC_CAPS_DISPLAY_AREA_POSITION = 0x00000080,
        MC_CAPS_DISPLAY_AREA_SIZE = 0x00000100,
        MC_CAPS_RESTORE_FACTORY_DEFAULTS = 0x00000400,
        MC_CAPS_RESTORE_FACTORY_COLOR_DEFAULTS = 0x00000800,
        MC_RESTORE_FACTORY_DEFAULTS_ENABLES_MONITOR_SETTINGS = 0x00001000
    }

    // found on https://github.com/emoacht/Monitorian/blob/master/Source/Monitorian.Core/Models/Monitor/MonitorConfiguration.cs
    [Flags]
    private enum MC_SUPPORTED_COLOR_TEMPERATURE
    {
        MC_SUPPORTED_COLOR_TEMPERATURE_NONE = 0x00000000,
        MC_SUPPORTED_COLOR_TEMPERATURE_4000K = 0x00000001,
        MC_SUPPORTED_COLOR_TEMPERATURE_5000K = 0x00000002,
        MC_SUPPORTED_COLOR_TEMPERATURE_6500K = 0x00000004,
        MC_SUPPORTED_COLOR_TEMPERATURE_7500K = 0x00000008,
        MC_SUPPORTED_COLOR_TEMPERATURE_8200K = 0x00000010,
        MC_SUPPORTED_COLOR_TEMPERATURE_9300K = 0x00000020,
        MC_SUPPORTED_COLOR_TEMPERATURE_10000K = 0x00000040,
        MC_SUPPORTED_COLOR_TEMPERATURE_11500K = 0x00000080
    }

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

    /// <summary>
    /// Returns a list of all attached screens on this machine
    /// </summary>
    public ServiceResult<IList<ScreenDto>> GetScreens()
    {
        var screens = new List<Screen>();

        foreach (var display in WindowsDisplayAPI.Display.GetDisplays())// for each windows display
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

            DestroyPhysicalMonitors(1, physicalMonitors);

            screens.Add(new Screen()
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
                MinBrightness = (int)min,
                MaxBrightness = (int)max,
                CurrentBrightness = (int)current,
            });
        }

        return new ServiceResult<IList<ScreenDto>>()
        {
            Success = true,
            Data = screens.Select(x => x.ToDto()).ToList()
        };
    }

    private ServiceResult<bool> ApplyRGBToScreen(float red, float green, float blue, string devicePath)
    {
        var array = new ushort[3 * 256];

        for (var ik = 0; ik < 256; ik++)
        {
            array[ik] = (ushort)(ik * red);
            array[256 + ik] = (ushort)(ik * green);
            array[512 + ik] = (ushort)(ik * blue);
        }

        var display = WindowsDisplayAPI.Display.GetDisplays().FirstOrDefault(x => x.DevicePath == devicePath);

        if (display == null) return new ServiceResult<bool>()
        {
            Success = false,
            Errors = [$"Could not find screen '{devicePath}'."]
        };

        var hdc = CreateDC(display.DisplayName, display.DevicePath, null, IntPtr.Zero);

        var pinnedArray = GCHandle.Alloc(array, GCHandleType.Pinned);
        IntPtr pointer = pinnedArray.AddrOfPinnedObject();

        var succeeded = SetDeviceGammaRamp(hdc.ToInt32(), pointer);

        DeleteDC(hdc);

        pinnedArray.Free();

        return new ServiceResult<bool>()
        {
            Success = succeeded,
            Errors = !succeeded ? ["Value is not supported."] : null
        };
    }

    /// <summary>
    /// Changes screen color from kelvin value
    /// Thanks to Tanner Helland for his algorithm http://www.tannerhelland.com/4435/convert-temperature-rgb-algorithm-code/
    /// </summary>
    public ServiceResult<bool> ApplyKelvinToScreen(int value, string devicePath)
    {
        if(value < 1000 || value > 40000)
        {
            return new ServiceResult<bool>()
            {
                Success = false,
                Errors = ["Invalid value."]
            };
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
            if (green < 0)
            {
                green = 0;
            }

            if (green > 255)
            {
                green = 255;
            }
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

    public ServiceResult<bool> ApplyColorToScreen(string stringColor, string devicePath)
    {
        ColorConverter converter = new ColorConverter();
        var color = (Color?)converter.ConvertFromString(stringColor);

        if (!color.HasValue)
        {
            return new ServiceResult<bool>()
            {
                Success = false,
                Errors = ["Invalid color."]
            };
        }

        return ApplyRGBToScreen(color.Value.R, color.Value.G, color.Value.B, devicePath);
    }

    public ServiceResult<bool> ApplyBrightnessToScreen(int brightness, string devicePath)
    {
        if (!GetScreenPhysicalMonitor(devicePath, out PHYSICAL_MONITOR[] physicalMonitors)) return new ServiceResult<bool>()
        {
            Success = false,
            Errors = [$"Could not find screen '{devicePath}'."]
        };

        var succeeded = false;

        if(GetMonitorCapabilities(physicalMonitors[0].hPhysicalMonitor, out MC_CAPS pdwMonitorCapabilities, out MC_SUPPORTED_COLOR_TEMPERATURE pdwSupportedColorTemperatures))
        {
            if(pdwMonitorCapabilities.HasFlag(MC_CAPS.MC_CAPS_BRIGHTNESS))
            {
                uint min = 0, max = 0, current = 0;

                GetMonitorBrightness(physicalMonitors[0].hPhysicalMonitor, ref min, ref current, ref max);

                // calculate maximum when minimum is 0
                var maximum = max - min;

                // brightness is in percentage
                var valueToApply = (uint)brightness * maximum / 100 + min;

                succeeded = SetMonitorBrightness(physicalMonitors[0].hPhysicalMonitor, valueToApply);
            }
        }

        DestroyPhysicalMonitors(1, physicalMonitors);

        return new ServiceResult<bool>()
        {
            Success = succeeded,
        };
    }
}