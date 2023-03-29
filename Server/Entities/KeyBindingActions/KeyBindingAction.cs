
using System.ComponentModel.DataAnnotations.Schema;

namespace ScreenTemperature.Entities.KeyBindingActions;

[InterfaceType]
public abstract class KeyBindingAction
{
    /// <summary>
    /// Returns the identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Returns the identifier of the associated <see cref="Entities.KeyBinding"/>.
    /// </summary>
    public int KeyBindingId { get; set; }

    /// <summary>
    /// Returns the associated <see cref="Entities.KeyBinding"/>.
    /// </summary>
    public KeyBinding KeyBinding { get; set; }
}