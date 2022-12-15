using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.AspNetCore.Mvc;
using ScreenTemperature.Dto;
using ScreenTemperature.Services;

namespace ScreenTemperature.Controllers;

public class ScreensController : Controller
{
    private readonly ILogger<ScreensController> _logger;
    private readonly IScreensService _screenService;

    public ScreensController(ILogger<ScreensController> logger, IScreensService screenService)
    {
        _logger = logger;
        _screenService = screenService;
    }

    /// <summary>
    /// Get the screens
    /// </summary>
    public IEnumerable<ScreenDto> GetScreens()
    {
        return _screenService.GetScreens();
    }
}
