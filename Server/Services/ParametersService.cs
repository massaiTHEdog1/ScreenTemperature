using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using ScreenTemperature.DTOs;
using ScreenTemperature.DTOs.Configurations;
using ScreenTemperature.Entities;
using ScreenTemperature.Entities.Configurations;
using System.Diagnostics;
using System.Reflection;

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

    public async Task<ParametersDto> UpdateParametersAsync(ParametersDto newParameters)
    {
        var currentParameters = await GetParametersAsync();

        if (newParameters.StartApplicationOnUserLogin != currentParameters.StartApplicationOnUserLogin)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(_runKeyPath, true))
            {
                if (key != null)
                {
                    var applicationName = Process.GetCurrentProcess().ProcessName;

                    if (newParameters.StartApplicationOnUserLogin)
                    {
                        var applicationPath = Assembly.GetExecutingAssembly().Location;

                        key.SetValue(applicationName, applicationPath);
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
        }

        if (newParameters.MinimizeOnStartup != currentParameters.MinimizeOnStartup)
        {
            var minimizeOnStartupKeyValue = await databaseContext.Parameters.SingleOrDefaultAsync(x => x.Key == _minimizeOnStartupKey);

            if (minimizeOnStartupKeyValue == null)
            {
                minimizeOnStartupKeyValue = new Parameter() { Key = _minimizeOnStartupKey };

                databaseContext.Parameters.Add(minimizeOnStartupKeyValue);
            }

            minimizeOnStartupKeyValue.Value = newParameters.MinimizeOnStartup.ToString();

            await databaseContext.SaveChangesAsync();
        }

        return newParameters;
    }
}