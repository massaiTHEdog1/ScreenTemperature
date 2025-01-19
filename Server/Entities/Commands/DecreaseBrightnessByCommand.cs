
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScreenTemperature.Entities.Commands;

/// <summary>
/// Action to increase the brightness of a list of <see cref="Screen"/>.
/// </summary>
//public class DecreaseBrightnessBy : KeyBindingAction
//{
//    /// <summary>
//    /// Returns a list of <see cref="Screen"/> that are targetted by the <see cref="Action"/>.
//    /// </summary>
//    public List<Screen> Targets { get; set; }

//    /// <summary>
//    /// Returns the value to set.
//    /// </summary>
//    public int Value { get; set; }
//}