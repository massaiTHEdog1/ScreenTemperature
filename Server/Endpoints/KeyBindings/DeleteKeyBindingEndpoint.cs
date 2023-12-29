using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using ScreenTemperature.DTOs;
using ScreenTemperature.Services;

public class DeleteKeyBindingEndpoint : EndpointWithoutRequest<Results<Ok, BadRequest<APIErrorResponseDto>>>
{
    private readonly IKeyBindingService _keyBindingService;

    public DeleteKeyBindingEndpoint(IKeyBindingService keyBindingService)
    {
        _keyBindingService = keyBindingService;
    }

    public override void Configure()
    {
        Delete("/api/keybindings/{Id}");
        AllowAnonymous();
    }

    public override async Task<Results<Ok, BadRequest<APIErrorResponseDto>>> ExecuteAsync(CancellationToken ct)
    {
        var result = await _keyBindingService.DeleteKeyBindingAsync(Route<Guid>("Id"), ct);

        if (!result.Success) return TypedResults.BadRequest(new APIErrorResponseDto(result.Errors));

        return TypedResults.Ok();
    }
}