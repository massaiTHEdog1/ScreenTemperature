using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ScreenTemperature.DTOs;
using ScreenTemperature.Services;
using System.ComponentModel.DataAnnotations;

[ApiController]
[AllowAnonymous]
public class ProfileController
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("/api/profiles")]
    public async Task<Results<Ok<IList<ProfileDto>>, BadRequest<APIErrorResponseDto>>> GetAllAsync()
    {
        var result = await _profileService.GetAllAsync();

        if (!result.Success) return TypedResults.BadRequest(new APIErrorResponseDto(result.Errors));

        return TypedResults.Ok(result.Data);
    }

    [HttpPut("/api/profiles/{id}")]
    public async Task<Results<Ok<ProfileDto>, BadRequest<APIErrorResponseDto>>> CreateOrUpdateAsync([Required] Guid id, [FromBody][Required] ProfileDto dto)
    {
        if(id != dto.Id) return TypedResults.BadRequest(new APIErrorResponseDto([$"{nameof(ProfileDto.Id)} mismatch."]));

        var result = await _profileService.CreateOrUpdateAsync(dto);

        if (!result.Success) return TypedResults.BadRequest(new APIErrorResponseDto(result.Errors));

        return TypedResults.Ok(result.Data);
    }

    [HttpDelete("/api/profiles/{id}")]
    public async Task<Results<Ok, BadRequest<APIErrorResponseDto>>> DeleteAsync([Required] Guid id)
    {
        var result = await _profileService.DeleteAsync(id);

        if (!result.Success) return TypedResults.BadRequest(new APIErrorResponseDto(result.Errors));

        return TypedResults.Ok();
    }

    [HttpPost("/api/profiles/{id}/apply")]
    public async Task<Results<Ok<IList<ConfigurationApplyResultDto>>, BadRequest<APIErrorResponseDto>>> ApplyProfileAsync([Required] Guid id)
    {
        var result = await _profileService.ApplyAsync(id);

        if (!result.Success) return TypedResults.BadRequest(new APIErrorResponseDto(result.Errors));

        return TypedResults.Ok(result.Data);
    }
}