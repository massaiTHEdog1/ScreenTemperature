using System.Text.Json.Serialization;

namespace ScreenTemperature.DTOs.Configurations
{
    public enum ConfigurationDiscriminator
    {
        TemperatureConfiguration = 0,
        ColorConfiguration = 1,
    }

    [JsonDerivedType(derivedType: typeof(TemperatureConfigurationDto), typeDiscriminator: ((int)ConfigurationDiscriminator.TemperatureConfiguration))]
    [JsonDerivedType(derivedType: typeof(ColorConfigurationDto), typeDiscriminator: ((int)ConfigurationDiscriminator.ColorConfiguration))]
    public abstract class ConfigurationDto
    {
        public Guid Id { get; set; }
        public string DevicePath { get; set; }
        public byte Brightness { get; set; }
    }
}
