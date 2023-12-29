using System.Text.Json.Serialization;

namespace ScreenTemperature.DTOs.KeyBindingActions
{
    [JsonDerivedType(derivedType: typeof(ApplyProfileActionDto), "applyProfile")]
    public abstract class KeyBindingActionDto
    {
        public Guid Id { get; set; }
    }
}
