using ScreenTemperature.DTOs.Configurations;
using ScreenTemperature.Entities.Configurations;

namespace ScreenTemperature.Mappers
{
    public static class ConfigurationMapperExtensions
    {
        public static Configuration ToEntity(this ConfigurationDto dto)
        {
            if(dto == null) throw new ArgumentNullException(nameof(dto));

            if(dto is TemperatureConfigurationDto temperatureConfigurationDto)
            {
                return new TemperatureConfiguration()
                {
                    Id = temperatureConfigurationDto.Id,
                    DevicePath = temperatureConfigurationDto.DevicePath,
                    Intensity = temperatureConfigurationDto.Intensity,
                    Brightness = temperatureConfigurationDto.Brightness,
                };
            }
            else if(dto is ColorConfigurationDto colorConfigurationDto)
            {
                return new ColorConfiguration()
                {
                    Id = colorConfigurationDto.Id,
                    DevicePath = colorConfigurationDto.DevicePath,
                    Color = colorConfigurationDto.Color,
                    Brightness = colorConfigurationDto.Brightness,
                };
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static ConfigurationDto ToDto(this Configuration entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            if (entity is TemperatureConfiguration temperatureConfiguration)
            {
                return new TemperatureConfigurationDto()
                {
                    Id = temperatureConfiguration.Id,
                    DevicePath = temperatureConfiguration.DevicePath,
                    Intensity = temperatureConfiguration.Intensity,
                    Brightness = temperatureConfiguration.Brightness,
                };
            }
            else if (entity is ColorConfiguration colorConfiguration)
            {
                return new ColorConfigurationDto()
                {
                    Id = colorConfiguration.Id,
                    DevicePath = colorConfiguration.DevicePath,
                    Color = colorConfiguration.Color,
                    Brightness = colorConfiguration.Brightness,
                };
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
