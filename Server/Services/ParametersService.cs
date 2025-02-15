using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using ScreenTemperature.Entities;
using System.Diagnostics;
using System.Reflection;

namespace ScreenTemperature.Services;

public interface IParametersService
{
    Parameters GetParameters();
    Parameters UpdateParameters(Parameters parameters);
}

public class ParametersService(ILogger<ParametersService> logger) : IParametersService
{
    private const string _runKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public Parameters GetParameters()
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

        return new Parameters()
        {
            StartApplicationOnUserLogin = startAppOnUserLogin,
        };
    }

    public Parameters UpdateParameters(Parameters newParameters)
    {
        var currentParameters = GetParameters();

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

        return newParameters;
    }
}