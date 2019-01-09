using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ScreenTemperature.Classes;
using ScreenTemperature.Services.Interfaces;

namespace ScreenTemperature.Services
{
    class MonitorService : IMonitorService
    {
        #region Variables

        private List<Monitor> _monitors;

        #endregion

        #region DLLs

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

        #endregion

        #region Constructor

        //public MonitorService()
        //{

        //}

        #endregion

        #region Methods

        public List<Monitor> GetMonitors(bool refresh = false)
        {
            if (_monitors == null || refresh)
            {
                _monitors = new List<Monitor>();

                IntPtr hdc = CreateDC("DISPLAY", null, null, IntPtr.Zero);

                _monitors.Add(new Monitor()
                {
                    Name = "All screens",
                    Index = 0,
                    Hdc = hdc
                });

                for (int i = 0; i < Screen.AllScreens.Length; i++)
                {
                    hdc = CreateDC(Screen.AllScreens[i].DeviceName, null, null, IntPtr.Zero);

                    _monitors.Add(new Monitor()
                    {
                        Name = "Screen " + (i+1),
                        Index = i+1,
                        Hdc = hdc
                    });
                }
            }

            return _monitors;
        }

        public Monitor GetAllMonitorsInOne()
        {
            return GetMonitors()[0];
        }

        public List<Monitor> GetMonitorsExceptAllMonitorsInOne()
        {
            return GetMonitors().Skip(1).ToList();
        }

        public IntPtr GetHdcByMonitorIndex(int monitorIndex)
        {
            Monitor monitor = GetMonitors().FirstOrDefault(x => x.Index == monitorIndex);

            if (monitor == null)
            {
                return IntPtr.Zero;
            }
            else
            {
                return monitor.Hdc;
            }
        }

        #endregion
    }
}
