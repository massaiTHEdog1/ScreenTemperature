using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;

namespace ScreenTemperature.Classes
{
    [Serializable]
    public class Monitor : INotifyPropertyChanged
    {
        #region Variables

        private string _label;
        private string _deviceName;
        private bool _isRadioButtonUseTannerHellandAlgorithmChecked = true;
        private int _tannerHellandSliderValue = 6600;
        private bool _isRadioButtonUseImageChecked;
        private Color _customColor = new Color() { A = 255, R = 255, G = 187, B = 127 };
        private BitmapImage _imageGradient;
        private int _customColorSliderValue = 1000;
        private bool _tannerValueIsInvalid;
        private bool _customColorIsInvalid;

        #endregion

        #region Properties

        [JsonIgnore]
        public bool IgnoreRadioValueChange { get; set; }

        [JsonIgnore]
        public bool CustomColorIsInvalid
        {
            get => _customColorIsInvalid;
            set
            {
                _customColorIsInvalid = value;
                NotifyPropertyChanged(nameof(CustomColorIsInvalid));
            }
        }

        [JsonIgnore]
        public bool TannerValueIsInvalid 
        {
            get => _tannerValueIsInvalid;
            set
            {
                _tannerValueIsInvalid = value;
                NotifyPropertyChanged(nameof(TannerValueIsInvalid));
            }
        }

        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                NotifyPropertyChanged(nameof(Label));
            }
        }

        public string DeviceName
        {
            get => _deviceName;
            set
            {
                _deviceName = value;
                NotifyPropertyChanged(nameof(DeviceName));
            }
        }

        public bool IsRadioButtonUseTannerHellandAlgorithmChecked
        {
            get => _isRadioButtonUseTannerHellandAlgorithmChecked;
            set
            {
                if (IgnoreRadioValueChange)
                    return;

                if (!value)
                    TannerValueIsInvalid = false;

                _isRadioButtonUseTannerHellandAlgorithmChecked = value;
                NotifyPropertyChanged(nameof(IsRadioButtonUseTannerHellandAlgorithmChecked));

                PropertyChangedApplyMonitor?.Invoke();
            }
        }

        public int TannerHellandSliderValue
        {
            get => _tannerHellandSliderValue;
            set
            {
                _tannerHellandSliderValue = value;
                NotifyPropertyChanged(nameof(TannerHellandSliderValue));

                PropertyChangedApplyMonitor?.Invoke();
            }
        }

        public bool IsRadioButtonUseImageChecked
        {
            get => _isRadioButtonUseImageChecked;
            set
            {
                if (IgnoreRadioValueChange)
                    return;

                if (!value)
                    CustomColorIsInvalid = false;

                _isRadioButtonUseImageChecked = value;
                NotifyPropertyChanged(nameof(IsRadioButtonUseImageChecked));

                PropertyChangedApplyMonitor?.Invoke();
            }
        }

        public Color CustomColor
        {
            get => _customColor;
            set
            {
                _customColor = value;
                NotifyPropertyChanged(nameof(CustomColor));

                GenerateImageGradient();

                PropertyChangedApplyMonitor?.Invoke();
            }
        }

        [JsonIgnore]
        public BitmapImage ImageGradient
        {
            get => _imageGradient;
            set
            {
                _imageGradient = value;
                NotifyPropertyChanged(nameof(ImageGradient));
            }
        }

        public int CustomColorSliderValue
        {
            get => _customColorSliderValue;
            set
            {
                _customColorSliderValue = value;
                NotifyPropertyChanged(nameof(CustomColorSliderValue));

                PropertyChangedApplyMonitor?.Invoke();
            }
        }

        #endregion

        public event Action PropertyChangedApplyMonitor;

        public Monitor()
        {
            GenerateImageGradient();
        }

        [JsonIgnore]
        public IntPtr Hdc;

        #region Implémentation INotifyPropertyChanged

        //INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private void GenerateImageGradient()
        {
            using (var bitmap = new Bitmap(1000, 1))
            using (var graphics = Graphics.FromImage(bitmap))
            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Color.FromArgb(CustomColor.A, CustomColor.R, CustomColor.G, CustomColor.B),
                System.Drawing.Color.White,
                LinearGradientMode.Horizontal))
            using (var memory = new MemoryStream())
            {
                graphics.FillRectangle(brush, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                ImageGradient = bitmapImage;
            }
        }

        public Monitor Clone()
        {
            return new Monitor()
            {
                Label = Label,
                DeviceName = DeviceName,
                IsRadioButtonUseTannerHellandAlgorithmChecked = IsRadioButtonUseTannerHellandAlgorithmChecked,
                TannerHellandSliderValue = TannerHellandSliderValue,
                IsRadioButtonUseImageChecked = IsRadioButtonUseImageChecked,
                CustomColor = CustomColor,
                CustomColorSliderValue = CustomColorSliderValue,
                ImageGradient = ImageGradient
            };
        }

        public void CopyTo(Monitor target)
        {
            target.Label = Label;
            target.DeviceName = DeviceName;
            target.IsRadioButtonUseTannerHellandAlgorithmChecked = IsRadioButtonUseTannerHellandAlgorithmChecked;
            target.TannerHellandSliderValue = TannerHellandSliderValue;
            target.IsRadioButtonUseImageChecked = IsRadioButtonUseImageChecked;
            target.CustomColor = CustomColor;
            target.CustomColorSliderValue = CustomColorSliderValue;
            target.ImageGradient = ImageGradient;
        }
    }
}
