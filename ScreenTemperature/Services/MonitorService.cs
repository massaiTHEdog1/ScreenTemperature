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
		private static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

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

				for (int i = 0; i < Screen.AllScreens.Length; i++)
				{
					var hdc = CreateDC(Screen.AllScreens[i].DeviceName, null, null, IntPtr.Zero);

					_monitors.Add(new Monitor()
					{
						DeviceName = Screen.AllScreens[i].DeviceName,
						Label = "Screen " + (i + 1),
						Hdc = hdc
					});
				}
			}

			return _monitors;
		}

		public IntPtr GetHdcByMonitorName(string name)
		{
			var monitor = GetMonitors().FirstOrDefault(x => x.DeviceName == name);

			return monitor?.Hdc ?? IntPtr.Zero;
		}

		#endregion
	}
}
