
using ScreenTemperature.Entities.Commands;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScreenTemperature.Entities;

/// <summary>
/// Key binding
/// </summary>
public class KeyBinding
{
    /// <summary>
    /// Returns the identifier.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    /// <summary>
    /// Returns the commands associated to this binding.
    /// </summary>
    public List<Command> Commands { get; set; }

    public string Name { get; set; }

    /// <summary>
    /// Return the key that executes the <see cref="Commands"/>.
    /// </summary>
    public int KeyCode { get; set; }

    /// <summary>
    /// Returns whether the Alt key should be pressed.
    /// </summary>
    public bool Alt { get; set; }

    /// <summary>
    /// Returns whether the Shift key should be pressed.
    /// </summary>
    public bool Shift { get; set; }

    /// <summary>
    /// Returns whether the Ctrl should be pressed.
    /// </summary>
    public bool Control { get; set; }
}