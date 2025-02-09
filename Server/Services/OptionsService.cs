using Microsoft.Win32;
using ScreenTemperature.Entities;
using System.Diagnostics;
using System.Reflection;

namespace ScreenTemperature.Services;

public interface IOptionsService
{
    /// <summary>
    /// Get the options
    /// </summary>
    Options GetOptions();

    /// <summary>
    /// Set the start application on user log in value
    /// </summary>
    bool SetStartAppOnUserLogin(bool startAppOnUserLogin);
}

public class OptionsService(ILogger<OptionsService> logger) : IOptionsService
{
    private const string _runKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>
    /// Get the options
    /// </summary>
    public Options GetOptions()
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

        return new Options()
        {
            StartApplicationOnUserLogin = startAppOnUserLogin,
        };
    }

    /// <summary>
    /// Set the start application on user log in value
    /// </summary>
    public bool SetStartAppOnUserLogin(bool startAppOnUserLogin)
    {
        using (var key = Registry.CurrentUser.OpenSubKey(_runKeyPath, true))
        {
            if (key != null)
            {
                var applicationName = Process.GetCurrentProcess().ProcessName;

                if(startAppOnUserLogin)
                {
                    var applicationPath = Assembly.GetExecutingAssembly().Location;

                    key.SetValue(applicationName, applicationPath);

                    return true;
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

        return false;
    }
}