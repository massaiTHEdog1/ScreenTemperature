using ScreenTemperature.DTOs;
using ScreenTemperature.DTOs.Configurations;
using ScreenTemperature.Entities;
using ScreenTemperature.Entities.Configurations;

namespace ScreenTemperature.Mappers
{
    public static class ProfileMapperExtensions
    {
        public static Profile ToEntity(this ProfileDto dto)
        {
            return new Profile()
            {
                Id = dto.Id,
                Label = dto.Label,
                Configurations = dto.Configurations?.Select(x => x.ToEntity())?.ToList() ?? new List<Configuration>()
            };
        }

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
