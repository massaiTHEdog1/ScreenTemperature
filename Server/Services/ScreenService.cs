
using ScreenTemperature.Entities;

namespace ScreenTemperature.Services;

public interface IScreenService
{
    /// <summary>
    /// Returns a list of all attached screens on this machine
    /// </summary>
    IEnumerable<Screen> GetScreens();
}

public class ScreenService : IScreenService
{
    private readonly ILogger<ScreenService> _logger;

    public ScreenService(ILogger<ScreenService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Returns a list of all attached screens on this machine
    /// </summary>
    public IEnumerable<Screen> GetScreens()
    {
        var screens = new List<Screen>();

        foreach(var display in WindowsDisplayAPI.Display.GetDisplays())
        {
            var displayTarget = display.ToPathDisplayTarget();

            screens.Add(new Screen()
            {
                Label = $"({screens.Count + 1}) - {displayTarget.FriendlyName}",
                Width = display.CurrentSetting.Resolution.Width,
                Height = display.CurrentSetting.Resolution.Height,
                IsPrimary = display.IsGDIPrimary,
                X = display.CurrentSetting.Position.X,
                Y = display.CurrentSetting.Position.Y,
                DevicePath = display.DevicePath,
            });
        }

        return screens;
    }
}