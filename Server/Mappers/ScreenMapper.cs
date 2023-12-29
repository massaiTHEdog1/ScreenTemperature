using ScreenTemperature.DTOs;
using ScreenTemperature.DTOs.Configurations;
using ScreenTemperature.Entities;
using ScreenTemperature.Entities.Configurations;

namespace ScreenTemperature.Mappers
{
    public static class ScreenMapperExtensions
    {
        public static ScreenDto ToDto(this Screen entity)
        {
            return new ScreenDto()
            {
                DevicePath = entity.DevicePath,
                Height = entity.Height,
                Width = entity.Width,
                IsPrimary = entity.IsPrimary,
                X = entity.X,
                Y = entity.Y,
                Label = entity.Label,
            };
        }
    }
}
