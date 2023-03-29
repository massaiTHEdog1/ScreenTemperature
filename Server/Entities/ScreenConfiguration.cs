
namespace ScreenTemperature.Entities;

/// <summary>
/// The configuration of a <see cref="Screen"/>.
/// </summary>
public class ScreenConfiguration
{
    /// <summary>
    /// Returns the identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Returns the identifier of the associated <see cref="Entities.Configuration"/>.
    /// </summary>
    public int ConfigurationId { get; set; }

    /// <summary>
    /// Returns the associated <see cref="Entities.Configuration"/>.
    /// </summary>
    public Configuration Configuration { get; set; }

    /// <summary>
    /// Returns the identifier of the <see cref="Screen"/> this configuration is associated with.
    /// </summary>
    public string DevicePath { get; set; }
}