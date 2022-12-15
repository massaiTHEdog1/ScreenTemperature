using Microsoft.AspNetCore.Mvc;
using ScreenTemperature.Dto;
using ScreenTemperature.Services;

namespace ScreenTemperature.Controllers;

public class OptionsController : Controller
{
    private readonly ILogger<OptionsController> _logger;
    private readonly IOptionsService _optionService;

    public OptionsController(ILogger<OptionsController> logger, IOptionsService optionService)
    {
        _logger = logger;
        _optionService = optionService;
    }

    /// <summary>
    /// Get the options
    /// </summary>
    public OptionsDto GetOptions()
    {
        return _optionService.GetOptions();
    }

    /// <summary>
    /// Set the start application on user log in value
    /// </summary>
    [HttpPost]
    public bool SetStartAppOnUserLogin([FromBody] PrimitiveDto<bool> dto)
    {
        return _optionService.SetStartAppOnUserLogin(dto.Value);
    }
}
