using Microsoft.AspNetCore.Mvc;
using ScreenTemperature.Dto;
using ScreenTemperature.Services;

namespace ScreenTemperature.Controllers;

public class ConfigurationsController : Controller
{
    private readonly ILogger<ConfigurationsController> _logger;
    private readonly IConfigurationsService _configurationService;

    public ConfigurationsController(ILogger<ConfigurationsController> logger, IConfigurationsService configurationService)
    {
        _logger = logger;
        _configurationService = configurationService;
    }

    /// <summary>
    /// Get user configurations
    /// </summary>
    public async Task<IEnumerable<ConfigurationDto>> GetConfigurations()
    {
        return await _configurationService.GetConfigurationsAsync();
    }

    /// <summary>
    /// Set the key binding of a configuration
    /// </summary>
    /// <param name="keyBindingDto">Key binding to set</param>
    [HttpPost]
    public async Task<KeyBindingDto> SetConfigurationKeyBinding([FromBody] KeyBindingDto keyBindingDto)
    {
        return await _configurationService.SetConfigurationKeyBinding(keyBindingDto);
    }
}
