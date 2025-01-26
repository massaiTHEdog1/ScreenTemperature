using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScreenTemperature;
using ScreenTemperature.DTOs;
using ScreenTemperature.DTOs.Configurations;
using ScreenTemperature.Entities.Configurations;
using ScreenTemperature.Mappers;
using ScreenTemperature.Services;
using System.ComponentModel.DataAnnotations;

[AllowAnonymous]
public class ConfigurationController
{
    private readonly ILogger<ConfigurationController> _logger;
    private readonly DatabaseContext _databaseContext;
    private readonly IScreenService _screenService;

    public ConfigurationController(ILogger<ConfigurationController> logger, DatabaseContext databaseContext, IScreenService screenService)
    {
        _logger = logger;
        _databaseContext = databaseContext;
        _screenService = screenService;
    }



    [HttpGet("/api/configurations")]
    public async Task<IResult> GetAllAsync()
    {
        var entities = await _databaseContext.Configurations.ToListAsync();

        return TypedResults.Ok(entities.Select(x => x.ToDto()));
    }



    [HttpPut("/api/configurations/{id:guid}")]
    public async Task<IResult> CreateOrUpdateAsync([Required] Guid id, [FromBody][Required] ConfigurationDto dto)
    {
        if(id != dto.Id) return TypedResults.BadRequest(new APIErrorResponseDto([$"{nameof(ConfigurationDto.Id)} mismatch."]));

        // todo : Add dto validation

        var entity = await _databaseContext.Configurations.FirstOrDefaultAsync(x => x.Id == dto.Id);

        if (entity == null)
        {
            if (dto is TemperatureConfigurationDto)
                entity = new TemperatureConfiguration() { Id = dto.Id };
            else if (dto is ColorConfigurationDto)
                entity = new ColorConfiguration() { Id = dto.Id };
            else
                throw new NotImplementedException();

            _databaseContext.Configurations.Add(entity);
        }

        entity.Name = dto.Name;
        entity.DevicePath = dto.DevicePath;
        entity.ApplyBrightness = dto.ApplyBrightness;
        entity.Brightness = dto.Brightness;

        if (entity is ColorConfiguration colorConfiguration && dto is ColorConfigurationDto colorConfigurationDto)
        {
            colorConfiguration.ApplyColor = colorConfigurationDto.ApplyColor;
            colorConfiguration.Color = colorConfigurationDto.Color;
        }
        else if (entity is TemperatureConfiguration temperatureConfiguration && dto is TemperatureConfigurationDto temperatureConfigurationDto)
        {
            temperatureConfiguration.ApplyIntensity = temperatureConfigurationDto.ApplyIntensity;
            temperatureConfiguration.Intensity = temperatureConfigurationDto.Intensity;
        }
        else
        {
            return TypedResults.BadRequest(new APIErrorResponseDto([$"Type of configuration {dto.Id} is invalid."]));
        }

        await _databaseContext.SaveChangesAsync();

        return TypedResults.Ok(entity.ToDto());
    }



    [HttpDelete("/api/configurations/{id:guid}")]
    public async Task<IResult> DeleteAsync([Required] Guid id)
    {
        var entity = await _databaseContext.Configurations.SingleOrDefaultAsync(x => x.Id == id);

        if (entity == null) return TypedResults.BadRequest(new APIErrorResponseDto(["This configuration does not exist."]));

        _databaseContext.Configurations.Remove(entity);

        await _databaseContext.SaveChangesAsync();

        return TypedResults.Ok();
    }



    //[HttpPost("/api/configurations/apply")]
    //public async Task<IResult> ApplyAsync([FromBody][Required] ConfigurationDto dto)
    //{
    //    if (dto.ApplyBrightness)
    //        _screenService.ApplyBrightnessToScreenAsync(dto.Brightness, dto.DevicePath);

    //    if (dto is TemperatureConfigurationDto temperatureConfiguration)
    //    {
    //        if (temperatureConfiguration.ApplyIntensity)
    //            _screenService.ApplyKelvinToScreenAsync(temperatureConfiguration.Intensity, temperatureConfiguration.DevicePath);
    //    }
    //    else if (dto is ColorConfigurationDto colorConfiguration)
    //    {
    //        if (colorConfiguration.ApplyColor)
    //            _screenService.ApplyColorToScreenAsync(colorConfiguration.Color, colorConfiguration.DevicePath);
    //    }
    //    else
    //    {
    //        throw new NotImplementedException();
    //    }

    //    return TypedResults.Ok();
    //}
}