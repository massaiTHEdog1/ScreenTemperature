using ScreenTemperature.DTOs.Configurations;
using ScreenTemperature.Entities.Configurations;

namespace ScreenTemperature.Mappers
{
    public static class ConfigurationMapperExtensions
    {
        public static ConfigurationDto ToDto(this Configuration entity)
        {
            if (entity is TemperatureConfiguration temperatureConfiguration)
            {
                return new TemperatureConfigurationDto()
                {
                    Id = temperatureConfiguration.Id,
                    Name = temperatureConfiguration.Name,
                    DevicePath = temperatureConfiguration.DevicePath,
                    Intensity = temperatureConfiguration.Intensity,
                    Brightness = temperatureConfiguration.Brightness,
                    ApplyBrightness = temperatureConfiguration.ApplyBrightness,
                    ApplyIntensity = temperatureConfiguration.ApplyIntensity,
                };
            }
            else if (entity is ColorConfiguration colorConfiguration)
            {
                return new ColorConfigurationDto()
                {
                    Id = colorConfiguration.Id,
                    Name = colorConfiguration.Name,
                    DevicePath = colorConfiguration.DevicePath,
                    Color = colorConfiguration.Color,
                    Brightness = colorConfiguration.Brightness,
                    ApplyBrightness = colorConfiguration.ApplyBrightness,
                    ApplyColor = colorConfiguration.ApplyColor,
                };
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
