
namespace ScreenTemperature.Entities;

public class KeyBinding
{
    public int Id { get; set; }

    public int ConfigurationId { get; set; }
    public Configuration Configuration { get; set; }

    /// <summary>
    /// The key
    /// </summary>
    public char Key { get; set; }

    /// <summary>
    /// Alt key pressed
    /// </summary>
    public bool Alt { get; set; }

    /// <summary>
    /// Shift key pressed
    /// </summary>
    public bool Shift { get; set; }

    /// <summary>
    /// Control key pressed
    /// </summary>
    public bool Control { get; set; }
}