using System;
using System.Collections.Generic;
using ScreenTemperature.Classes;

namespace ScreenTemperature.Services.Interfaces
{
    interface IMonitorService
    {
        List<Monitor> GetMonitors();
        Monitor GetAllMonitorsInOne();
        List<Monitor> GetMonitorsExceptAllMonitorsInOne();
        IntPtr GetHdcByMonitorName(string monitorName);
    }
}
