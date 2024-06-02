using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ScreenTemperature.DTOs;
using ScreenTemperature.Services;

[AllowAnonymous]
public class ScreenController : Controller
{
    private readonly IScreenService _screenService;

    public ScreenController(IScreenService screenService)
    {
        _screenService = screenService;
    }

    [HttpGet("/api/screens")]
    public async Task<Results<Ok<IList<ScreenDto>>, BadRequest<APIErrorResponseDto>>> GetAllScreensAsync()
    {
        var getScreensResult = _screenService.GetScreens();

        if (!getScreensResult.Success) return TypedResults.BadRequest(new APIErrorResponseDto(getScreensResult.Errors));

        return TypedResults.Ok(getScreensResult.Data);
    }
}