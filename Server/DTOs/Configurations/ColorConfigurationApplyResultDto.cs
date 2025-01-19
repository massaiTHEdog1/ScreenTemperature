using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace ScreenTemperature.DTOs.Configurations
{
    public class ColorConfigurationApplyResultDto : ConfigurationApplyResultDto
    {
        public bool SucceededToApplyColor { get; set; }
        public string[] ApplyColorErrors { get; set; }
    }
}
