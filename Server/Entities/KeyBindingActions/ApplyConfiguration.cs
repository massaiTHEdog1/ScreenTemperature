
using System.ComponentModel.DataAnnotations;

namespace ScreenTemperature.Entities.KeyBindingActions;

/// <summary>
/// Action to apply a <see cref="Entities.Profile"/>.
/// </summary>
public class ApplyConfiguration : KeyBindingAction
{
    /// <summary>
    /// Returns the <see cref="Entities.Profile"/> to apply.
    /// </summary>
    [Required]
    public Profile Configuration { get; set; }
}