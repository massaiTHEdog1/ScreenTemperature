using ScreenTemperature.Dto;
using ScreenTemperature.Entities;

namespace ScreenTemperature.Mappings;

public static partial class Mapper
{
    public static KeyBindingDto Map(KeyBinding source)
    {
        return new KeyBindingDto()
        {
            Alt = source.Alt,
            Control = source.Control,
            ConfigurationId = source.ConfigurationId,
            Id = source.Id,
            Key = source.Key,
            Shift = source.Shift,
        };
    }

    public static KeyBinding Map(KeyBindingDto source)
    {
        return new KeyBinding()
        {
            Alt = source.Alt,
            Control = source.Control,
            ConfigurationId = source.ConfigurationId,
            Id = source.Id,
            Key = source.Key,
            Shift = source.Shift,
        };
    }
}