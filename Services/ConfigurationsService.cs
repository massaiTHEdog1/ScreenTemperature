using Microsoft.EntityFrameworkCore;
using ScreenTemperature.Dto;
using ScreenTemperature.Entities;
using ScreenTemperature.Exceptions;

namespace ScreenTemperature.Services;

public interface IConfigurationsService
{
    /// <summary>
    /// Get user configurations
    /// </summary>
    Task<IEnumerable<ConfigurationDto>> GetConfigurationsAsync();
    /// <summary>
    /// Set the key binding of a configuration
    /// </summary>
    /// <param name="keyBindingDto">Key binding to set</param>
    Task<KeyBindingDto> SetConfigurationKeyBinding(KeyBindingDto keyBindingDto);
}

public class ConfigurationsService : IConfigurationsService
{
    private readonly ILogger<ConfigurationsService> _logger;
    private readonly DatabaseContext _databaseContext;

    public ConfigurationsService(ILogger<ConfigurationsService> logger, DatabaseContext databaseContext)
    {
        _logger = logger;
        _databaseContext = databaseContext;
    }

    /// <summary>
    /// Get user configurations
    /// </summary>
    public async Task<IEnumerable<ConfigurationDto>> GetConfigurationsAsync()
    {
        var dtos = new List<ConfigurationDto>();

        var configurations = await _databaseContext.Configurations.ToListAsync();

        return dtos;
    }

    /// <summary>
    /// Set the key binding of a configuration
    /// </summary>
    /// <param name="keyBindingDto">Key binding to set</param>
    public async Task<KeyBindingDto> SetConfigurationKeyBinding(KeyBindingDto keyBindingDto)
    {
        var configuration = await _databaseContext.Configurations.SingleOrDefaultAsync(x => x.Id == keyBindingDto.ConfigurationId);

        if (configuration == null)
            throw new BadRequestException($"Invalid {nameof(keyBindingDto.ConfigurationId)} value.");

        throw new NotImplementedException();
    }
}