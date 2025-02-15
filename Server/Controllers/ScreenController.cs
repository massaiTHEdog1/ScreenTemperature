using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ScreenTemperature.DTOs;
using ScreenTemperature.Mappers;
using ScreenTemperature.Services;

public class ScreenController(IScreenService screenService)
{
    [HttpGet("/api/screens")]
    public async Task<IResult> GetAllScreensAsync()
    {
        var screens = await screenService.GetScreensAsync();

        return TypedResults.Ok(screens.Select(x => x.ToDto()));
    }
}