using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using ScreenTemperature.Classes;
using ScreenTemperature.Services.Interfaces;

namespace ScreenTemperature.Services
{
	class ScreenColorService : IScreenColorService
	{
		#region Variables



		#endregion

		#region Properties



		#endregion

		#region Services

		private readonly IConfigService _configService;
		private readonly IMonitorService _monitorService;

		#endregion

		#region DLLs

		[DllImport("GDI32.dll")]
		private static extern unsafe bool SetDeviceGammaRamp(int hdc, void* ramp);

		[DllImport("GDI32.dll")]
		private static extern unsafe bool GetDeviceGammaRamp(int hdc, void* ramp);

		#endregion

		#region Constructor

		public ScreenColorService(IConfigService configService, IMonitorService monitorService)
		{
			_configService = configService;
			_monitorService = monitorService;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Changes screen color from kelvin value
		/// Thanks to Tanner Helland for his algorithm http://www.tannerhelland.com/4435/convert-temperature-rgb-algorithm-code/
		/// </summary>
		public unsafe void ChangeScreenColorFromKelvin(int value, Monitor monitor)
		{
			float kelvin = value;
			var temperature = kelvin / 100;

			float red, green, blue;

			if (temperature <= 66)
			{
				red = 255;
			}
			else
			{
				red = temperature - 60;
				red = 329.698727446f * ((float)Math.Pow(red, -0.1332047592));
				if (red < 0) red = 0;
				if (red > 255) red = 255;
			}

			if (temperature <= 66)
			{
				green = temperature;
				green = 99.4708025861f * (float)Math.Log(green) - 161.1195681661f;
				if (green < 0)
				{
					green = 0;
				}

				if (green > 255)
				{
					green = 255;
				}
			}
			else
			{
				green = temperature - 60;
				green = 288.1221695283f * ((float)Math.Pow(green, -0.0755148492));

				if (green < 0) green = 0;
				if (green > 255) green = 255;
			}


			if (temperature >= 66)
			{
				blue = 255;
			}
			else
			{
				if (temperature <= 19)
				{
					blue = 0;
				}
				else
				{
					blue = temperature - 10;
					blue = 138.5177312231f * (float)Math.Log(blue) - 305.0447927307f;
					if (blue < 0) blue = 0;
					if (blue > 255) blue = 255;
				}
			}

			if (value == 6600)
			{
				red = 255;
				green = 255;
				blue = 255;
			}

			ushort* gArray = stackalloc ushort[3 * 256];

			for (var ik = 0; ik < 256; ik++)
			{
				gArray[ik] = (ushort)(ik * red);
				gArray[256 + ik] = (ushort)(ik * green);
				gArray[512 + ik] = (ushort)(ik * blue);
			}

			SetDeviceGammaRamp(monitor.Hdc.ToInt32(), gArray);
		}

		/// <summary>
		/// Changes screen color from a Config
		/// </summary>
		public unsafe void ChangeScreenColorFromConfig(Config config)
		{
			ushort* gArray = stackalloc ushort[3 * 256];

			foreach (var monitor in _monitorService.GetMonitors())
			{
				ChangeScreenColorFromKelvin(6600, monitor);
			}

			foreach(var monitor in config.Monitors)
			{
				for (var i = 0; i < 256 * 3; i++)
				{
					gArray[i] = monitor.Rgb[i];
				}

				SetDeviceGammaRamp(_monitorService.GetHdcByMonitorName(monitor.Name).ToInt32(), gArray);
			}
		}

		public unsafe Config SaveCurrentScreenColorToConfig(string configName)
		{
			List<Config> configs = _configService.GetConfigs();
			var configToModify = configs.FirstOrDefault(x => x.ConfigName == configName);

			//If the config doesn't exist
			if (configToModify == null) 
			{
				configToModify = new Config()
				{
					ConfigName = configName == "" ? "config" : configName,
					Monitors = new List<Monitor>(),
					Order = configs.Count
				};
			}
			//If the config already exists
			else
			{
				if (MessageBox.Show($"Are you sure you want to erase this config: {configName}?", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
				{
					configToModify.Monitors = new List<Monitor>();
				}
				else
				{
					return null;
				}
			}

			List<Monitor> monitors = _monitorService.GetMonitorsExceptAllMonitorsInOne();

			foreach (var monitor in monitors)
			{
				ushort* gArray = stackalloc ushort[3 * 256];

				//Get screen data
				var retVal = GetDeviceGammaRamp(monitor.Hdc.ToInt32(), gArray);

				//If it' ok
				if (retVal) 
				{
					List<ushort> rgb = new List<ushort>();

					for (var i = 0; i < 256 * 3; i++)
					{
						rgb.Add(gArray[i]);
					}

					configToModify.Monitors.Add(new Monitor()
					{
						Name = monitor.Name,
						Rgb = rgb.ToArray()
					});
				}
				else
				{
					MessageBox.Show("Can't get screen data. \r\nTry again or restart application.", "Error");

					return null;
				}
			}

			return _configService.SaveConfig(configToModify);
		}

		/// <summary>
		/// Changes screen color from an image
		/// </summary>
		public unsafe void ChangeScreenColorFromImage(int value, Monitor monitor, BitmapImage image)
		{
			float red, green, blue;

			using (var outStream = new MemoryStream())
			{
				BitmapEncoder enc = new BmpBitmapEncoder();
				enc.Frames.Add(BitmapFrame.Create(image));
				enc.Save(outStream);

				using (var bmp = new Bitmap(outStream))
				{
					if (value < 0)
						value = 0;
					if (value >= bmp.Width)
						value = bmp.Width - 1;


					var clr = bmp.GetPixel(value, 0);

					red = clr.R;
					green = clr.G;
					blue = clr.B;
				}
			}

			ushort* gArray = stackalloc ushort[3 * 256];

			for (var ik = 0; ik < 256; ik++)
			{
				gArray[ik] = (ushort)(ik * red);
				gArray[256 + ik] = (ushort)(ik * green);
				gArray[512 + ik] = (ushort)(ik * blue);
			}

			SetDeviceGammaRamp(monitor.Hdc.ToInt32(), gArray);
		}

		#endregion
	}
}
