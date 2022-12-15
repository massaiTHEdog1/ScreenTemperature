using System.Diagnostics.CodeAnalysis;

namespace ScreenTemperature.Entities;

public class Configuration
{
    public int Id { get; set; }

    public string Label { get; set; }

    /// <summary>
    /// The associated key binding
    /// </summary>
    public KeyBinding KeyBinding { get; set; }

    /// <summary>
    /// The configuration of each screen
    /// </summary>
    public IEnumerable<ScreenConfig> Configs { get; set; }
}