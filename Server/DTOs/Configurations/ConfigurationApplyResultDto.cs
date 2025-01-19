using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace ScreenTemperature.DTOs.Configurations
{
    [SwaggerDiscriminator("$type")]
    [SwaggerSubType(typeof(TemperatureConfigurationApplyResultDto), DiscriminatorValue = "temperature")]
    [JsonDerivedType(typeof(TemperatureConfigurationApplyResultDto), "temperature")]
    [SwaggerSubType(typeof(ColorConfigurationApplyResultDto), DiscriminatorValue = "color")]
    [JsonDerivedType(typeof(ColorConfigurationApplyResultDto), "color")]
    public abstract class ConfigurationApplyResultDto
    {
        public string DevicePath { get; set; }
        public bool SucceededToApplyBrightness { get; set; }
        public string[] ApplyBrightnessErrors { get; set; }
    }
}
