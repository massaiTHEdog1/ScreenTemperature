using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenTemperature.DTOs
{
    public class ParametersDto
    {
        /// <summary>
        /// Returns whether the application should be started when the user logs in.
        /// </summary>
        public bool StartApplicationOnUserLogin { get; set; }

        public bool MinimizeOnStartup { get; set; }
    }
}
