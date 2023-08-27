
using System.ComponentModel.DataAnnotations.Schema;

namespace ScreenTemperature.Entities.KeyBindingActions;

public abstract class KeyBindingAction
{
    /// <summary>
    /// Returns the identifier.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    /// <summary>
    /// Returns the identifier of the associated <see cref="Entities.KeyBinding"/>.
    /// </summary>
    public Guid KeyBindingId { get; set; }

    /// <summary>
    /// Returns the associated <see cref="Entities.KeyBinding"/>.
    /// </summary>
    public KeyBinding KeyBinding { get; set; }
}