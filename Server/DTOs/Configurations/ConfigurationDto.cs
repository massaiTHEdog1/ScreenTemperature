using System.Text.Json.Serialization;

namespace ScreenTemperature.DTOs.Configurations
{
    [JsonDerivedType(derivedType: typeof(TemperatureConfigurationDto), "temperature")]
    [JsonDerivedType(derivedType: typeof(ColorConfigurationDto), "color")]
    public abstract class ConfigurationDto
    {
        public Guid Id { get; set; }
        public string DevicePath { get; set; }
        public bool ApplyBrightness { get; set; }
        public byte Brightness { get; set; }
    }
}
