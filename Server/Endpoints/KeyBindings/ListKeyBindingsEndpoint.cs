using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using ScreenTemperature.DTOs;
using ScreenTemperature.Services;

public class ListKeyBindingsEndpoint : EndpointWithoutRequest<Results<Ok<IList<KeyBindingDto>>, BadRequest<APIErrorResponseDto>>>
{
    private readonly IKeyBindingService _keyBindingService;

    public ListKeyBindingsEndpoint(IKeyBindingService keyBindingService)
    {
        _keyBindingService = keyBindingService;
    }

    public override void Configure()
    {
        Get("/api/keybindings");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<IList<KeyBindingDto>>, BadRequest<APIErrorResponseDto>>> ExecuteAsync(CancellationToken ct)
    {
        var result = await _keyBindingService.ListKeyBindingsAsync(ct);

        if (!result.Success) return TypedResults.BadRequest(new APIErrorResponseDto(result.Errors));

        return TypedResults.Ok(result.Data);
    }
}