using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using ScreenTemperature.Classes;
using ScreenTemperature.Services.Interfaces;

namespace ScreenTemperature.Services
{
    class ConfigService : IConfigService
    {
        #region Variables

        private readonly string _configDirectory;
        private List<Config> _listConfigs;

        #endregion

        #region Properties

        private List<Config> ListConfigs
        {
            get
            {
                if (_listConfigs == null)
                {
                    _listConfigs = GetConfigs();
                }

                return _listConfigs;
            }
        }

        #endregion

        #region Constructor

        public ConfigService()
        {
            _configDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ScreenTemperature\Configs";

            try
            {
                Directory.CreateDirectory(_configDirectory);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Can't create config directory.\r\nTry restarting application.\r\nErrorr:\r\n{e.Message}", "Error");
                Environment.Exit(0);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all the user's configs
        /// </summary>
        public List<Config> GetConfigs()
        {
            if (_listConfigs == null)
            {
                _listConfigs = new List<Config>();

                foreach (var file in Directory.GetFiles(_configDirectory, "*.bin"))
                {
                    try
                    {
                        IFormatter formatter = new BinaryFormatter();

                        using (Stream stream =
                            new FileStream($@"{file}", FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            _listConfigs.Add((Config) formatter.Deserialize(stream));
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"File {file} corrupt.\r\nCan't use this config.\r\nError:\r\n{e.Message}",
                            "Error");
                    }
                }
            }

            return _listConfigs;
        }

        /// <summary>
        /// Gets a config by it's name
        /// </summary>
        public Config GetConfigByName(string name)
        {
            return ListConfigs.FirstOrDefault(x => x.ConfigName == name);
        }

        /// <summary>
        /// Saves the config
        /// </summary>
        public Config SaveConfig(Config config)
        {
            var addConfigToList = false;

            if (string.IsNullOrEmpty(config.ConfigPath))//if this config doesn't exist
            {
                var i = 0;

                while (true)
                {
                    if (!File.Exists($@"{_configDirectory}\config{i}.bin"))
                    {
                        using (var fs = File.Create($@"{_configDirectory}\config{i}.bin"))
                        {
                            fs.Close();
                        }

                        config.ConfigPath = $@"{_configDirectory}\config{i}.bin";
                        addConfigToList = true;
                        break;
                    }

                    i++;
                }

                config.ConfigName = string.IsNullOrEmpty(config.ConfigName) ? $"config{i}" : config.ConfigName;
            }

            try
            {
                IFormatter formatter = new BinaryFormatter();

                using (Stream stream = new FileStream(config.ConfigPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    formatter.Serialize(stream, config);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Impossible d'enregistrer le fichier config.\r\nErreur:\r\n" + exception.Message, "Erreur");
            }

            if(addConfigToList)
                ListConfigs.Add(config);

            return config;
        }

        /// <summary>
        /// Deletes the config
        /// </summary>
        public bool DeleteConfig(Config config)
        {
            try
            {
                File.Delete(config.ConfigPath);
                ListConfigs.Remove(config);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}
