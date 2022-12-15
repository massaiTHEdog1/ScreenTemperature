namespace ScreenTemperature.Dto;

public class ScreenConfigDto
{
    public int Id { get; set; }

    public int ConfigurationId { get; set; }

    /// <summary>
    /// Identifier of the screen this configuration is associated with
    /// </summary>
    public string DevicePath { get; set; }
}