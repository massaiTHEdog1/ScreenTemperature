using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ScreenTemperature.DTOs;
using ScreenTemperature.DTOs.Configurations;
using ScreenTemperature.Services;
using System.ComponentModel.DataAnnotations;

[ApiController]
[AllowAnonymous]
public class ConfigurationController
{
    private readonly IConfigurationService _configurationService;

    public ConfigurationController(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    [HttpGet("/api/configurations")]
    public async Task<Results<Ok<IList<ConfigurationDto>>, BadRequest<APIErrorResponseDto>>> GetAllAsync()
    {
        var result = await _configurationService.GetAllAsync();

        if (!result.Success) return TypedResults.BadRequest(new APIErrorResponseDto(result.Errors));

        return TypedResults.Ok(result.Data);
    }

    [HttpPut("/api/configurations/{id}")]
    public async Task<Results<Ok<ConfigurationDto>, BadRequest<APIErrorResponseDto>>> CreateOrUpdateAsync([Required] Guid id, [FromBody][Required] ConfigurationDto dto)
    {
        if(id != dto.Id) return TypedResults.BadRequest(new APIErrorResponseDto([$"{nameof(ConfigurationDto.Id)} mismatch."]));

        var result = await _configurationService.CreateOrUpdateAsync(dto);

        if (!result.Success) return TypedResults.BadRequest(new APIErrorResponseDto(result.Errors));

        return TypedResults.Ok(result.Data);
    }

    [HttpDelete("/api/configurations/{id}")]
    public async Task<Results<Ok, BadRequest<APIErrorResponseDto>>> DeleteAsync([Required] Guid id)
    {
        var result = await _configurationService.DeleteAsync(id);

        if (!result.Success) return TypedResults.BadRequest(new APIErrorResponseDto(result.Errors));

        return TypedResults.Ok();
    }

    [HttpPost("/api/configurations/apply")]
    public async Task<Results<Ok<ConfigurationApplyResultDto>, BadRequest<APIErrorResponseDto>>> ApplyAsync([FromBody][Required] ConfigurationDto dto)
    {
        var result = await _configurationService.ApplyAsync(dto);

        if (!result.Success) return TypedResults.BadRequest(new APIErrorResponseDto(result.Errors));

        return TypedResults.Ok(result.Data);
    }
}