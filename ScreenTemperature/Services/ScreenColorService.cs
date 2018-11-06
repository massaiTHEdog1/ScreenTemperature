using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using ScreenTemperature.Classes;
using ScreenTemperature.Services.Interfaces;

namespace ScreenTemperature.Services
{
    class ScreenColorService : IScreenColorService
    {
        #region Variables

        private Graphics _graphics;
        private IntPtr _hdc;

        #endregion

        #region Properties

        private IntPtr Hdc
        {
            get
            {
                if (_hdc == IntPtr.Zero)
                {
                    _graphics = Graphics.FromHwnd(IntPtr.Zero);
                    _hdc = _graphics.GetHdc();

                    return _hdc;
                }
                else
                {
                    return _hdc;
                }
            }
        }

        #endregion

        #region Services

        private IConfigService _configService;

        #endregion

        #region DLLs

        [DllImport("GDI32.dll")]
        private static extern unsafe bool SetDeviceGammaRamp(Int32 hdc, void* ramp);

        [DllImport("GDI32.dll")]
        private static extern unsafe bool GetDeviceGammaRamp(Int32 hdc, void* ramp);

        #endregion

        #region Constructor

        public ScreenColorService(IConfigService configService)
        {
            _configService = configService;
        }

        ~ScreenColorService()
        {
            if (_graphics != null)
            {
                //_graphics.ReleaseHdc(_hdc);
                _graphics.Dispose();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Changes screen color from kelvin value
        /// </summary>
        public unsafe void ChangeScreenColorFromKelvin(int value)
        {
            float kelvin = value;
            float temperature = kelvin / 100;

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

            short* gArray = stackalloc short[3 * 256];

            for (int ik = 0; ik < 256; ik++)
            {
                gArray[ik] = (short)(ik * red);
                gArray[256 + ik] = (short)(ik * green);
                gArray[512 + ik] = (short)(ik * blue);
            }

            SetDeviceGammaRamp(Hdc.ToInt32(), gArray);
        }

        /// <summary>
        /// Changes screen color from a Config
        /// </summary>
        public unsafe void ChangeScreenColorFromConfig(Config config)
        {
            short* gArray = stackalloc short[3 * 256];

            ChangeScreenColorFromKelvin(6600);

            for (int i = 0; i < 256 * 3; i++)
            {
                gArray[i] = config.Rgb[i];
            }

            SetDeviceGammaRamp(Hdc.ToInt32(), gArray);
        }

        public unsafe Config SaveCurrentScreenColorToConfig(string configName)
        {
            short* gArray = stackalloc short[3 * 256];

            bool retVal = GetDeviceGammaRamp(Hdc.ToInt32(), gArray);//Get screen data

            if (retVal)//If it' ok
            {
                List<short> rgb = new List<short>();

                for (int i = 0; i < 256 * 3; i++)
                {
                    rgb.Add(gArray[i]);
                }

                List<Config> configs = _configService.GetConfigs();

                Config configToModify = configs.FirstOrDefault(x => x.ConfigName == configName);

                if (configToModify == null)//If the config doesn't exist
                {
                    configToModify = new Config()
                    {
                        ConfigName = configName == "" ? "config" : configName,
                        Rgb = rgb.ToArray(),
                        Order = configs.Count
                    };

                    _configService.SaveConfig(configToModify);
                }
                else//If the config already exists
                {
                    if (MessageBox.Show($"Are you sure you want to erase this config: {configName}?", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        configToModify.Rgb = rgb.ToArray();

                        _configService.SaveConfig(configToModify);
                    }
                    else
                    {
                        return configToModify;//Maybe we just want to change the name so we souldn't change current screen color
                    }
                }

                return configToModify;
            }
            else
            {
                MessageBox.Show("Can't get screen data. \r\nTry again or restart application.", "Error");

                return null;
            }
        }

        #endregion
    }
}
