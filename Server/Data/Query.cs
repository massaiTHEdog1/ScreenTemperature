using ScreenTemperature.Entities;
using ScreenTemperature.Services;

namespace ScreenTemperature.Data;

public class Query
{
    // TODO : Implement DataLoader : https://chillicream.com/docs/hotchocolate/v13/fetching-data/dataloader

    /// <summary>
    /// Returns a list of <see cref="Configuration"/>.
    /// </summary>
    [UseProjection]
    [UseFiltering]
    public IQueryable<Configuration> GetConfigurations([Service] IConfigurationService configurationsRepository) => configurationsRepository.GetConfigurations();

    /// <summary>
    /// Returns a list of all attached screens on this machine.
    /// </summary>
    [UseFiltering]
    public IEnumerable<Screen> GetScreens([Service] IScreenService screenRepository) => screenRepository.GetScreens();

    /// <summary>
    /// Returns a list of <see cref="KeyBinding"/>.
    /// </summary>
    [UseProjection]
    [UseFiltering]
    public IQueryable<KeyBinding> GetKeyBindings([Service] IKeyBindingService keyBindingRepository) => keyBindingRepository.GetKeyBindings();
}