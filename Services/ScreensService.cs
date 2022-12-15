using ScreenTemperature.Dto;

namespace ScreenTemperature.Services;

public interface IScreensService
{
    /// <summary>
    /// Get the screens
    /// </summary>
    IEnumerable<ScreenDto> GetScreens();
}

public class ScreensService : IScreensService
{
    private readonly ILogger<ScreensService> _logger;

    public ScreensService(ILogger<ScreensService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get the screens
    /// </summary>
    public IEnumerable<ScreenDto> GetScreens()
    {
        var screens = new List<ScreenDto>();

        foreach(var display in WindowsDisplayAPI.Display.GetDisplays())
        {
            var displayTarget = display.ToPathDisplayTarget();

            screens.Add(new ScreenDto()
            {
                Label = !string.IsNullOrWhiteSpace(displayTarget.FriendlyName) ? displayTarget.FriendlyName : $"Screen {screens.Count+1}",
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