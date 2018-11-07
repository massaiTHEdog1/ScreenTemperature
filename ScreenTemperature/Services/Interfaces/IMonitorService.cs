using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
