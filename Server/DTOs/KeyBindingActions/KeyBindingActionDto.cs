using ScreenTemperature.DTOs.Configurations;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace ScreenTemperature.DTOs.KeyBindingActions
{
    [SwaggerDiscriminator("$type")]
    [SwaggerSubType(typeof(ApplyProfileActionDto), DiscriminatorValue = "applyProfile")]
    [JsonDerivedType(typeof(ApplyProfileActionDto), "applyProfile")]
    public abstract class KeyBindingActionDto
    {
        public Guid Id { get; set; }
    }
}
