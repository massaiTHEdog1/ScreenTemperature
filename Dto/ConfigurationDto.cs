using ScreenTemperature.Entities;
using System.Diagnostics.CodeAnalysis;

namespace ScreenTemperature.Dto;

public class ConfigurationDto
{
    public int Id { get; set; }

    public string Label { get; set; }

    /// <summary>
    /// The associated key binding
    /// </summary>
    public KeyBindingDto KeyBinding { get; set; }

    /// <summary>
    /// The configuration of each screen
    /// </summary>
    public IEnumerable<ScreenConfigDto> Configs { get; set; }
}