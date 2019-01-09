using System;
using System.ComponentModel;

namespace ScreenTemperature.Classes
{
    [Serializable]
    public class Monitor : INotifyPropertyChanged
    {
        private string _name;
        private int _index;
        private ushort[] _rgb;

        public string Name
        {
            get
            {
                return _name;
            }
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
            get { return _rgb; }
            set
            {
                _rgb = value;
                NotifyPropertyChanged("Rgb");
            }
        }

        [field: NonSerialized]
        public IntPtr Hdc;

        #region Implémentation INotifyPropertyChanged

        //INotifyPropertyChanged implementation
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
