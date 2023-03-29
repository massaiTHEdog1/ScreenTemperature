using Microsoft.EntityFrameworkCore;
using ScreenTemperature.Entities;
using ScreenTemperature.Entities.KeyBindingActions;
using ScreenTemperature.Exceptions;

namespace ScreenTemperature.Services;

public interface IConfigurationService
{
    /// <summary>
    /// Get configurations
    /// </summary>
    IQueryable<Configuration> GetConfigurations();

    /// <summary>
    ///  Create a <see cref="Configuration"/>
    /// </summary>
    Task<Configuration> AddConfigurationAsync(Configuration configuration);
}

public class ConfigurationService : IConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;
    private readonly DatabaseContext _databaseContext;

    public ConfigurationService(ILogger<ConfigurationService> logger, DatabaseContext databaseContext)
    {
        _logger = logger;
        _databaseContext = databaseContext;
    }

    /// <summary>
    /// Create a new <see cref="Configuration"/>.
    /// </summary>
    /// <param name="configuration">The <see cref="Configuration"/> to create.</param>
    public async Task<Configuration> AddConfigurationAsync(Configuration configuration)
    {
        if ((configuration.ScreensConfigurations?.Count() ?? 0) == 0)
            throw new ArgumentException($"{nameof(configuration.ScreensConfigurations)} is empty.");

        foreach(var screenConfiguration in configuration.ScreensConfigurations ?? new List<ScreenConfiguration>())
        {
            screenConfiguration.Id = 0;
        }

        _databaseContext.Configurations.Add(configuration);
        await _databaseContext.SaveChangesAsync();

        return configuration;
    }

    /// <summary>
    /// Returns a list of <see cref="Configuration"/>.
    /// </summary>
    public IQueryable<Configuration> GetConfigurations()
    {
        return _databaseContext.Configurations.AsQueryable();
    }
}