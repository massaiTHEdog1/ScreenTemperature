
namespace ScreenTemperature.Entities;

/// <summary>
/// Options of the application
/// </summary>
public class Options
{
    /// <summary>
    /// Returns whether the application should be started when the user logs in.
    /// </summary>
    public bool StartApplicationOnUserLogin { get; set; }
}