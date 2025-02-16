using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using ScreenTemperature.DTOs;
using ScreenTemperature.DTOs.Configurations;
using ScreenTemperature.Entities;
using ScreenTemperature.Entities.Configurations;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace ScreenTemperature.Services;

public interface IParametersService
{
    Task<ParametersDto> GetParametersAsync();
    Task<ParametersDto> UpdateParametersAsync(ParametersDto parameters);
}

public class ParametersService(ILogger<ParametersService> logger, DatabaseContext databaseContext) : IParametersService
{
    private const string _runKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string _minimizeOnStartupKey = "MinimizeOnStartup";

    public async Task<ParametersDto> GetParametersAsync()
    {
        var startAppOnUserLogin = false;

        using (var key = Registry.CurrentUser.OpenSubKey(_runKeyPath, true))
        {
            if (key != null)
            {
                var applicationName = Process.GetCurrentProcess().ProcessName;

                var subKey = key.GetValue(applicationName);

                startAppOnUserLogin = subKey != null;
            }
        }

        var minimizeOnStartupKeyValue = await databaseContext.Parameters.SingleOrDefaultAsync(x => x.Key == _minimizeOnStartupKey);

        return new ParametersDto()
        {
            StartApplicationOnUserLogin = startAppOnUserLogin,
            MinimizeOnStartup = minimizeOnStartupKeyValue != null ? bool.Parse(minimizeOnStartupKeyValue.Value) : false
        };
    }

    public async Task<ParametersDto> UpdateParametersAsync(ParametersDto dto)
    {
        var currentParameters = await GetParametersAsync();

        using (var key = Registry.CurrentUser.OpenSubKey(_runKeyPath, true))
        {
            if (key != null)
            {
                var applicationName = Process.GetCurrentProcess().ProcessName;

                if (dto.StartApplicationOnUserLogin)
                {
                    // Buffer to store the path
                    var pathBuffer = new StringBuilder(260); // MAX_PATH = 260

                    // Get the filename of the current module (pass IntPtr.Zero for current process)
                    uint result = Win32.GetModuleFileName(IntPtr.Zero, pathBuffer, (uint)pathBuffer.Capacity);

                    if (result > 0)
                        key.SetValue(applicationName, pathBuffer.ToString());
                }
                else
                {
                    var subKey = key.GetValue(applicationName);

                    if (subKey != null)
                    {
                        key.DeleteValue(applicationName);
                    }
                }
            }
        }

        var minimizeOnStartupKeyValue = await databaseContext.Parameters.SingleOrDefaultAsync(x => x.Key == _minimizeOnStartupKey);

        if (minimizeOnStartupKeyValue == null)
        {
            minimizeOnStartupKeyValue = new Parameter() { Key = _minimizeOnStartupKey };

            databaseContext.Parameters.Add(minimizeOnStartupKeyValue);
        }

        minimizeOnStartupKeyValue.Value = dto.MinimizeOnStartup.ToString();

        await databaseContext.SaveChangesAsync();

        return dto;
    }
}