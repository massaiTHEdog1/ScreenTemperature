using ScreenTemperature.Dto;
using ScreenTemperature.Entities;

namespace ScreenTemperature.Mappings;

public static partial class Mapper
{
    public static ConfigurationDto Map(Configuration source)
    {
        return new ConfigurationDto()
        {
            Id = source.Id,
            KeyBinding = source.KeyBinding == null ? null : Map(source.KeyBinding),
            Label = source.Label,
            Configs = source.Configs == null ? null : Map(source.Configs),
        };
    }

    public static Configuration Map(ConfigurationDto source)
    {
        return new Configuration()
        {
            Id = source.Id,
            KeyBinding = source.KeyBinding == null ? null : Map(source.KeyBinding),
            Label = source.Label,
            Configs = source.Configs == null ? null : Map(source.Configs),
        };
    }
}