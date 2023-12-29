using ScreenTemperature.DTOs;
using ScreenTemperature.DTOs.KeyBindingActions;
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
                Actions = entity.Actions?.Select(x => x.ToDto()) ?? new List<KeyBindingActionDto>(){},
                Alt = entity.Alt,
                Control = entity.Control,
                KeyCode = entity.KeyCode,
                Shift = entity.Shift
            };
        }
    }
}
