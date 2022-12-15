using ScreenTemperature.Dto;
using ScreenTemperature.Entities;

namespace ScreenTemperature.Mappings;

public static partial class Mapper
{
    public static ScreenConfigDto Map(ScreenConfig source)
    {
        return new ScreenConfigDto()
        {
            ConfigurationId = source.ConfigurationId,
            DevicePath = source.DevicePath,
            Id = source.Id,
        };
    }

    public static ScreenConfig Map(ScreenConfigDto source)
    {
        return new ScreenConfig()
        {
            ConfigurationId = source.ConfigurationId,
            DevicePath = source.DevicePath,
            Id = source.Id,
        };
    }

    public static IEnumerable<ScreenConfigDto> Map(IEnumerable<ScreenConfig> source)
    {
        return source.Select(x => Map(x));
    }

    public static IEnumerable<ScreenConfig> Map(IEnumerable<ScreenConfigDto> source)
    {
        return source.Select(x => Map(x));
    }
}