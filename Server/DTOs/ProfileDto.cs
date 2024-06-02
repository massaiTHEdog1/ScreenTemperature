using ScreenTemperature.DTOs.Configurations;
using System.Text.Json.Serialization;

namespace ScreenTemperature.DTOs
{
    public class ProfileDto
    {
        public Guid Id { get; set; }
        public string Label { get; set; }
        public IEnumerable<Guid> Configurations { get; set; }
    }
}