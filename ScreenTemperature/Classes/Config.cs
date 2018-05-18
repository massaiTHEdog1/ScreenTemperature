using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

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
                Save();
                NotifyPropertyChanged("Libelle");
            }
        }

        public short[] RGB
        {
            get { return _rgb; }
            set
            {
                _rgb = value;
                Save();
                NotifyPropertyChanged("RGB");
            }
        }

        public int Order
        {
            get { return _order; }
            set
            {
                _order = value;
                Save();
                NotifyPropertyChanged("Order");
            }
        }

        public string ConfigPath
        {
            get { return _configPath; }
            set
            {
                _configPath = value;
                Save();
                NotifyPropertyChanged("ConfigPath");
            }
        }

	    public KeyData KeyBinding
	    {
		    get { return _keyBinding; }
		    set
		    {
			    _keyBinding = value;
				Save();
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

        #region Constructeur

        public Config(string configName = null)
        {
            int i = 0;

            while (true)
            {
                if (!File.Exists($@"{MainWindow.configDirectory}\config{i}.bin"))
                {
                    using (FileStream fichier = File.Create($@"{MainWindow.configDirectory}\config{i}.bin"))
                    {
                        fichier.Close();
                    }

                    ConfigPath = $@"{MainWindow.configDirectory}\config{i}.bin";
                    break;
                }

                i++;
            }

            ConfigName = string.IsNullOrEmpty(configName) ? $"config{i}" : configName;
        }

        #endregion

        public override string ToString()
        {
            return ConfigName;
        }

        /// <summary>
        /// Enregistre la configuration de la classe dans un fichier binaire
        /// </summary>
        public void Save()
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();

                using (Stream stream = new FileStream(ConfigPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    formatter.Serialize(stream, this);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Impossible d'enregistrer le fichier config.\r\nErreur:\r\n" + exception.Message, "Erreur");
            }
        }

        public void Delete()
        {
            File.Delete(ConfigPath);
        }

        #endregion
    }
}
