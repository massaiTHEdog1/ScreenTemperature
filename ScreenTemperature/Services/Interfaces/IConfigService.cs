using System.Collections.Generic;
using ScreenTemperature.Classes;

namespace ScreenTemperature.Services.Interfaces
{
    interface IConfigService
    {
        /// <summary>
        /// Gets all the user's configs
        /// </summary>
        List<Config> GetConfigs();

        /// <summary>
        /// Gets a config by it's name
        /// </summary>
        Config GetConfigByName(string name);

        /// <summary>
        /// Saves the config
        /// </summary>
        Config SaveConfig(Config config);

        /// <summary>
        /// Deletes the config
        /// </summary>
        bool DeleteConfig(Config config);
    }
}
