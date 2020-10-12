using System.Windows.Media;

namespace HSVColorPickers
{
    //@Delegate
    /// <summary>
    /// Method that represents the handling of the ColorChange event.
    /// </summary>
    /// <param name="sender"> The object to which the event handler is attached. </param>
    /// <param name="value"> Event data. </param>
    public delegate void ColorChangeHandler(object sender, Color value);
}