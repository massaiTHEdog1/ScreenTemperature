using System;
using System.ComponentModel;

namespace ScreenTemperature.Classes
{
    [Serializable]
    public class Monitor : INotifyPropertyChanged
    {
        #region Variables

        private string _name;
        private int _index;
        private ushort[] _rgb;

        #endregion

        #region Properties

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                NotifyPropertyChanged("Name");
            }
        }

        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                _index = value;
                NotifyPropertyChanged("Index");
            }
        }

        public ushort[] Rgb
        {
            get => _rgb;
            set
            {
                _rgb = value;
                NotifyPropertyChanged("Rgb");
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
