using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace ScreenTemperature.DTOs.Configurations
{
    [SwaggerDiscriminator("$type")]
    [SwaggerSubType(typeof(TemperatureConfigurationDto), DiscriminatorValue = "temperature")]
    [JsonDerivedType(typeof(TemperatureConfigurationDto), "temperature")]
    [SwaggerSubType(typeof(ColorConfigurationDto), DiscriminatorValue = "color")]
    [JsonDerivedType(typeof(ColorConfigurationDto), "color")]
    public abstract class ConfigurationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DevicePath { get; set; }
        public bool ApplyBrightness { get; set; }
        public byte Brightness { get; set; }
        public bool ApplyAtStartup { get; set; }
    }
}
