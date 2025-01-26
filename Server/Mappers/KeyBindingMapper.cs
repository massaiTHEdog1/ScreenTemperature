using ScreenTemperature.DTOs;
using ScreenTemperature.DTOs.Commands;
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
                Commands = entity.Commands?.Select(x => x.ToDto()) ?? new List<CommandDto>(){},
                Alt = entity.Alt,
                Control = entity.Control,
                KeyCode = entity.KeyCode,
                Shift = entity.Shift
            };
        }
    }
}
