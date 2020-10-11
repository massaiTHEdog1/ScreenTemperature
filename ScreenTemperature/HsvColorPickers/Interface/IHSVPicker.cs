using System.Windows.Controls;

namespace HSVColorPickers
{
    /// <summary>
    /// Represents a basic Interface of HSV picker.
    /// </summary>
    public interface IHSVPicker
    {
        /// <summary> Gets picker's type name. </summary>
        string Type { get; }

        /// <summary> Gets picker self. </summary>
        Control Self { get; }

        /// <summary> Gets or sets picker's hsv. </summary>
        HSV HSV { get; set; }

        /// <summary> Occurs when the hsv value changed. </summary>
        event HSVChangeHandler HSVChanged;
        /// <summary> Occurs when the hsv change starts. </summary>
        event HSVChangeHandler HSVChangeStarted;
        /// <summary> Occurs when hsv change. </summary>
        event HSVChangeHandler HSVChangeDelta;
        /// <summary> Occurs when the hsv change is complete. </summary>
        event HSVChangeHandler HSVChangeCompleted;
    }
}