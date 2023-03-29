
using System.ComponentModel.DataAnnotations;

namespace ScreenTemperature.Entities;

/// <summary>
/// User-defined configuration for each <see cref="Screen"/>.
/// </summary>
public class Configuration
{
    /// <summary>
    /// Returns the identifier of this <see cref="Configuration"/>.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Returns the label of this <see cref="Configuration"/>.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Returns a <see cref="Configuration"/> for each <see cref="Screen"/>.
    /// </summary>
    public IEnumerable<ScreenConfiguration> ScreensConfigurations { get; set; }
}