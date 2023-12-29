
using ScreenTemperature.DTOs;
using ScreenTemperature.Entities;
using ScreenTemperature.Mappers;
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

    /// <summary>
    /// Returns a list of all attached screens on this machine
    /// </summary>
    public ServiceResult<IList<ScreenDto>> GetScreens()
    {
        var screens = new List<Screen>();

        foreach(var display in WindowsDisplayAPI.Display.GetDisplays())
        {
            var displayTarget = display.ToPathDisplayTarget();

            screens.Add(new Screen()
            {
                Label = displayTarget.FriendlyName,
                Width = display.CurrentSetting.Resolution.Width,
                Height = display.CurrentSetting.Resolution.Height,
                IsPrimary = display.IsGDIPrimary,
                X = display.CurrentSetting.Position.X,
                Y = display.CurrentSetting.Position.Y,
                DevicePath = display.DevicePath,
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

        pinnedArray.Free();

        return new ServiceResult<bool>()
        {
            Success = true,
            Data = succeeded
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
}