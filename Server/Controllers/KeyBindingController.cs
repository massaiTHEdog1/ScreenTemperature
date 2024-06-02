using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ScreenTemperature.DTOs;
using ScreenTemperature.Services;
using System.ComponentModel.DataAnnotations;

[ApiController]
[AllowAnonymous]
public class KeyBindingController
{
    private readonly IKeyBindingService _keyBindingService;

    public KeyBindingController(IKeyBindingService keyBindingService)
    {
        _keyBindingService = keyBindingService;
    }

    [HttpGet("/api/keybindings")]
    public async Task<Results<Ok<IList<KeyBindingDto>>, BadRequest<APIErrorResponseDto>>> GetAllAsync(CancellationToken ct)
    {
        var result = await _keyBindingService.ListKeyBindingsAsync(ct);

        if (!result.Success) return TypedResults.BadRequest(new APIErrorResponseDto(result.Errors));

        return TypedResults.Ok(result.Data);
    }

    [HttpPost("/api/keybindings")]
    public async Task<Results<Ok<KeyBindingWithHotKeyRegistrationResultDto>, BadRequest<APIErrorResponseDto>>> CreateAsync([Required] KeyBindingDto dto, CancellationToken ct)
    {
        var result = await _keyBindingService.CreateOrUpdateKeyBindingAsync(dto, ct);

        if (!result.Success) return TypedResults.BadRequest(new APIErrorResponseDto(result.Errors));

        return TypedResults.Ok(result.Data);
    }

    [HttpDelete("/api/keybindings/{id}")]
    public async Task<Results<Ok, BadRequest<APIErrorResponseDto>>> DeleteAsync([Required] Guid id, CancellationToken ct)
    {
        var result = await _keyBindingService.DeleteKeyBindingAsync(id, ct);

        if (!result.Success) return TypedResults.BadRequest(new APIErrorResponseDto(result.Errors));

        return TypedResults.Ok();
    }

    [HttpPut("/api/keybindings/{id}")]
    public async Task<Results<Ok<KeyBindingWithHotKeyRegistrationResultDto>, BadRequest<APIErrorResponseDto>>> UpdateAsync([Required] Guid id, [FromBody][Required] KeyBindingDto dto, CancellationToken ct)
    {
        if(id != dto.Id) return TypedResults.BadRequest(new APIErrorResponseDto([$"{nameof(KeyBindingDto.Id)} mismatch."]));

        var result = await _keyBindingService.CreateOrUpdateKeyBindingAsync(dto, ct);

        if (!result.Success) return TypedResults.BadRequest(new APIErrorResponseDto(result.Errors));

        return TypedResults.Ok(result.Data);
    }
}