namespace ScreenTemperature.Dto;

public class ScreenDto
{
    /// <summary>
    /// Identifier of the screen
    /// </summary>
    public string DevicePath { get; set; }

    /// <summary>
    /// Friendly name of the screen
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Is this the primary screen ?
    /// </summary>
    public bool IsPrimary { get; set; }

    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}