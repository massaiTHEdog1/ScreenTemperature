using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using ScreenTemperature.DTOs;
using ScreenTemperature.Services;

public class ListScreensEndpoint : EndpointWithoutRequest<Results<Ok<IList<ScreenDto>>, BadRequest<APIErrorResponseDto>>>
{
    private readonly IScreenService _screenService;

    public ListScreensEndpoint(IScreenService screenService)
    {
        _screenService = screenService;
    }

    public override void Configure()
    {
        Get("/api/screens");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<IList<ScreenDto>>, BadRequest<APIErrorResponseDto>>> ExecuteAsync(CancellationToken ct)
    {
        var getScreensResult = _screenService.GetScreens();

        if (!getScreensResult.Success) return TypedResults.BadRequest(new APIErrorResponseDto(getScreensResult.Errors));

        return TypedResults.Ok(getScreensResult.Data);
    }
}