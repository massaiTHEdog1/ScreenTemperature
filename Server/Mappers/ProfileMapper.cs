using ScreenTemperature.DTOs;
using ScreenTemperature.DTOs.Configurations;
using ScreenTemperature.Entities;
using ScreenTemperature.Entities.Configurations;

namespace ScreenTemperature.Mappers
{
    public static class ProfileMapperExtensions
    {
        public static ProfileDto ToDto(this Profile entity)
        {
            return new ProfileDto()
            {
                Id = entity.Id,
                Label = entity.Label,
                Configurations = entity.Configurations?.Select(x => x.ToDto()) ?? new List<ConfigurationDto>(),
            };
        }
    }
}
