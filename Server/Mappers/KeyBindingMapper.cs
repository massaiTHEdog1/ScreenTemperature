using ScreenTemperature.DTOs;
using ScreenTemperature.Entities;

namespace ScreenTemperature.Mappers
{
    public static class KeyBindingMapper
    {
        public static KeyBindingDto ToDto(this KeyBinding entity)
        {
            return new KeyBindingDto()
            {
                Id = entity.Id,
                Name = entity.Name,
                ConfigurationIds = entity.Configurations.Select(x => x.Id).ToList(),
                Alt = entity.Alt,
                Control = entity.Control,
                KeyCode = entity.KeyCode,
            };
        }
    }
}
