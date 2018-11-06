using System;
using System.ComponentModel;

namespace ScreenTemperature.Classes
{
	[Serializable]
    public class Config : INotifyPropertyChanged
    {
        #region Variables

        private string _configName;
        private short[] _rgb;
        private int _order;
        private string _configPath;
	    private KeyData _keyBinding;

	    #endregion;

        #region Propriétés

        public string ConfigName
        {
            get { return _configName; }
            set
            {
                _configName = value;
                NotifyPropertyChanged("Libelle");
            }
        }

        public short[] Rgb
        {
            get { return _rgb; }
            set
            {
                _rgb = value;
                NotifyPropertyChanged("RGB");
            }
        }

        public int Order
        {
            get { return _order; }
            set
            {
                _order = value;
                NotifyPropertyChanged("Order");
            }
        }

        public string ConfigPath
        {
            get { return _configPath; }
            set
            {
                _configPath = value;
                NotifyPropertyChanged("ConfigPath");
            }
        }

	    public KeyData KeyBinding
	    {
		    get { return _keyBinding; }
		    set
		    {
			    _keyBinding = value;
			    NotifyPropertyChanged("KeyBinding");
		    }
	    }

        #endregion

        #region Méthodes

        #region Implémentation INotifyPropertyChanged

        //INotifyPropertyChanged implementation
		[field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        public override string ToString()
        {
            return ConfigName;
        }

        #endregion
    }
}
