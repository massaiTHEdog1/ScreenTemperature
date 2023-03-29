
using System.ComponentModel.DataAnnotations;

namespace ScreenTemperature.Entities.KeyBindingActions;

/// <summary>
/// Action to apply a <see cref="Entities.Configuration"/>.
/// </summary>
public class ApplyConfiguration : KeyBindingAction
{
    /// <summary>
    /// Returns the <see cref="Entities.Configuration"/> to apply.
    /// </summary>
    [Required]
    public Configuration Configuration { get; set; }
}