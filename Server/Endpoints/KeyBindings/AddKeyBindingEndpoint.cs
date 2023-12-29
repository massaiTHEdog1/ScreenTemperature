using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using ScreenTemperature.DTOs;
using ScreenTemperature.Services;

public class AddKeyBindingEndpoint : Endpoint<KeyBindingDto, Results<Ok<KeyBindingWithHotKeyRegistrationResultDto>, BadRequest<APIErrorResponseDto>>>
{
    private readonly IKeyBindingService _keyBindingService;

    public AddKeyBindingEndpoint(IKeyBindingService keyBindingService)
    {
        _keyBindingService = keyBindingService;
    }

    public override void Configure()
    {
        Post("/api/keybindings");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<KeyBindingWithHotKeyRegistrationResultDto>, BadRequest<APIErrorResponseDto>>> ExecuteAsync(KeyBindingDto dto, CancellationToken ct)
    {
        var result = await _keyBindingService.CreateOrUpdateKeyBindingAsync(dto, ct);

        if (!result.Success) return TypedResults.BadRequest(new APIErrorResponseDto(result.Errors));

        return TypedResults.Ok(result.Data);
    }
}