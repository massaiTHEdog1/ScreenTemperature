using ScreenTemperature.DTOs.Configurations;

namespace ScreenTemperature.DTOs
{
    public class ProfileDto
    {
        public Guid Id { get; set; }
        public string Label { get; set; }
        public IEnumerable<ConfigurationDto> Configurations { get; set; }
    }
}