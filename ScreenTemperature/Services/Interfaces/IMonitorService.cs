using System;
using System.Collections.Generic;
using ScreenTemperature.Classes;

namespace ScreenTemperature.Services.Interfaces
{
    interface IMonitorService
    {
        List<Monitor> GetMonitors(bool refresh = false);
        Monitor GetAllMonitorsInOne();
        List<Monitor> GetMonitorsExceptAllMonitorsInOne();
        IntPtr GetHdcByMonitorIndex(int monitorIndex);
    }
}
