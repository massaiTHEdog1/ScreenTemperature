﻿using Microsoft.AspNetCore.SignalR;
using Microsoft.Win32;
using ScreenTemperature.Services;
using System.Diagnostics;
using System.Reflection;

namespace ScreenTemperature.Hubs;

public class Hub(IScreenService screenService, IParametersService optionsService) : Microsoft.AspNetCore.SignalR.Hub
{
    public async Task ApplyBrightness(int brightness, string devicePath)
    {
        var result = false;

        try
        {
            result = await screenService.ApplyBrightnessToScreenAsync(brightness, devicePath);
        }
        catch (Exception ex) { }
        
        await Clients.All.SendAsync("ApplyBrightnessResult", result);
    }

    public async Task ApplyTemperature(int temperature, string devicePath)
    {
        var result = false;

        try
        {
            result = await screenService.ApplyKelvinToScreenAsync(temperature, devicePath);
        }
        catch (Exception ex) { }

        await Clients.All.SendAsync("ApplyTemperatureResult", result);
    }

    public async Task ApplyColor(string color, string devicePath)
    {
        var result = false;

        try
        {
            result = await screenService.ApplyColorToScreenAsync(color, devicePath);
        }
        catch (Exception ex) { }

        await Clients.All.SendAsync("ApplyColorResult", result);
    }
}