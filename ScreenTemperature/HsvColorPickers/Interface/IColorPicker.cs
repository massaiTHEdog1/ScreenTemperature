using System.Windows.Controls;
using System.Windows.Media;

namespace HSVColorPickers
{
    /// <summary>
    /// Represents a basic Interface of color picker.
    /// </summary>
    public interface IColorPicker
    {
        /// <summary> Gets picker's type name. </summary>
        string Type { get; }

        /// <summary> Gets picker self. </summary>
        Control Self { get; }

        /// <summary> Gets or sets picker's color. </summary>
        Color Color { get; set; }

        /// <summary> Occurs when the color value changed. </summary>
        event ColorChangeHandler ColorChanged;
        /// <summary> Occurs when the color change starts. </summary>
        event ColorChangeHandler ColorChangeStarted;
        /// <summary> Occurs when color change. </summary>
        event ColorChangeHandler ColorChangeDelta;
        /// <summary> Occurs when the color change is complete. </summary>
        event ColorChangeHandler ColorChangeCompleted;
    }
}