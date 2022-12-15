
namespace ScreenTemperature.Entities;

public class ScreenConfig
{
    public int Id { get; set; }

    public int ConfigurationId { get; set; }
    public Configuration Configuration { get; set; }

    /// <summary>
    /// Identifier of the screen this configuration is associated with
    /// </summary>
    public string DevicePath { get; set; }
}