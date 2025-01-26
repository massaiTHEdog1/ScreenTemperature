using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ScreenTemperature.DTOs;
using ScreenTemperature.Mappers;
using ScreenTemperature.Services;

[AllowAnonymous]
public class ScreenController
{
    private readonly IScreenService _screenService;

    public ScreenController(IScreenService screenService)
    {
        _screenService = screenService;
    }

    [HttpGet("/api/screens")]
    public async Task<IResult> GetAllScreensAsync()
    {
        var screens = await _screenService.GetScreensAsync();

        return TypedResults.Ok(screens.Select(x => x.ToDto()));
    }
}