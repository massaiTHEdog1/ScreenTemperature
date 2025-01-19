using ScreenTemperature.DTOs.Configurations;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace ScreenTemperature.DTOs.Commands
{
    [SwaggerDiscriminator("$type")]
    public abstract class CommandDto
    {
        public Guid Id { get; set; }
    }
}
