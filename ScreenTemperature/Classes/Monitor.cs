using ScreenTemperature.Enums;
using System;
using System.ComponentModel;
using System.Windows.Media;

namespace ScreenTemperature.Classes
{
    [Serializable]
    public class Monitor : INotifyPropertyChanged
    {
        #region Variables

        private string _label;
        private string _deviceName;
        private AlgorithmType _algorithmType;
        private int _tannerHellandSliderValue;
        private Color _customColorColorValue;
        private int _customColorSliderValue;

        #endregion

        #region Properties

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

        public AlgorithmType AlgorithmType
        {
            get => _algorithmType;
            set
            {
                _algorithmType = value;
                NotifyPropertyChanged(nameof(AlgorithmType));
            }
        }

        public int TannerHellandSliderValue
        {
            get => _tannerHellandSliderValue;
            set
            {
                _tannerHellandSliderValue = value;
                NotifyPropertyChanged(nameof(TannerHellandSliderValue));
            }
        }

        public Color CustomColorColorValue
        {
            get => _customColorColorValue;
            set
            {
                _customColorColorValue = value;
                NotifyPropertyChanged(nameof(CustomColorColorValue));
            }
        }

        public int CustomColorSliderValue
        {
            get => _customColorSliderValue;
            set
            {
                _customColorSliderValue = value;
                NotifyPropertyChanged(nameof(CustomColorSliderValue));
            }
        }

        #endregion

        [field: NonSerialized]
        public IntPtr Hdc;

        #region Implémentation INotifyPropertyChanged

        //INotifyPropertyChanged implementation
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
