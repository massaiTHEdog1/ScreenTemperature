using Microsoft.EntityFrameworkCore;
using ScreenTemperature.DTOs.Configurations;
using ScreenTemperature.Entities.Configurations;
using ScreenTemperature.Mappers;
using System.Configuration;

namespace ScreenTemperature.Services;

public interface IConfigurationService
{
    Task<ServiceResult<IList<ConfigurationDto>>> GetAllAsync();
    Task<ServiceResult<ConfigurationDto>> CreateOrUpdateAsync(ConfigurationDto dto);
    Task<ServiceResult> DeleteAsync(Guid id);
    Task<ServiceResult<ConfigurationApplyResultDto>> ApplyAsync(ConfigurationDto dto);
}

public class ConfigurationService : IConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;
    private readonly DatabaseContext _databaseContext;
    private readonly IScreenService _screenService;

    public ConfigurationService(ILogger<ConfigurationService> logger, DatabaseContext databaseContext, IScreenService screenService)
    {
        _logger = logger;
        _databaseContext = databaseContext;
        _screenService = screenService;
    }

    public async Task<ServiceResult<IList<ConfigurationDto>>> GetAllAsync()
    {
        var entities = await _databaseContext.Configurations.ToListAsync();

        return new ServiceResult<IList<ConfigurationDto>>()
        {
            Success = true,
            Data = entities.Select(x => x.ToDto()).ToList()
        };
    }

    public async Task<ServiceResult<ConfigurationDto>> CreateOrUpdateAsync(ConfigurationDto dto)
    {
        if (dto == null) return new ServiceResult<ConfigurationDto>()
        {
            Success = false,
            Errors = ["Invalid parameter."]
        };

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

        if(entity is ColorConfiguration colorConfiguration && dto is ColorConfigurationDto colorConfigurationDto)
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
            return new ServiceResult<ConfigurationDto>()
            {
                Success = false,
                Errors = [$"Type of configuration {dto.Id} is invalid."]
            };
        }

        await _databaseContext.SaveChangesAsync();

        return new ServiceResult<ConfigurationDto>()
        {
            Success = true,
            Data = entity.ToDto(),
        };
    }

    public async Task<ServiceResult> DeleteAsync(Guid id)
    {
        if (id == Guid.Empty) return new ServiceResult() 
        {
            Success = false,
            Errors = ["Invalid parameter."]
        };
        
        var entity = await _databaseContext.Configurations.SingleOrDefaultAsync(x => x.Id == id);

        if(entity == null) return new ServiceResult()
        {
            Success = false,
            Errors = ["This configuration does not exist."]
        };

        _databaseContext.Configurations.Remove(entity);

        await _databaseContext.SaveChangesAsync();

        return new ServiceResult()
        {
            Success = true
        };
    }

    public async Task<ServiceResult<ConfigurationApplyResultDto>> ApplyAsync(ConfigurationDto dto)
    {
        ServiceResult<bool>? applyBrightnessResult = null;
        
        if (dto.ApplyBrightness)
            applyBrightnessResult = _screenService.ApplyBrightnessToScreen(dto.Brightness, dto.DevicePath);

        if (dto is TemperatureConfigurationDto temperatureConfiguration)
        {
            ServiceResult<bool>? applyTemperatureResult = null;

            if (temperatureConfiguration.ApplyIntensity)
                applyTemperatureResult = _screenService.ApplyKelvinToScreen(temperatureConfiguration.Intensity, temperatureConfiguration.DevicePath);

            return new ServiceResult<ConfigurationApplyResultDto>()
            {
                Success = true,
                Data = new TemperatureConfigurationApplyResultDto()
                {
                    DevicePath = dto.DevicePath,
                    SucceededToApplyBrightness = applyBrightnessResult?.Success ?? false,
                    ApplyBrightnessErrors = applyBrightnessResult?.Errors,
                    SucceededToApplyTemperature = applyTemperatureResult?.Success ?? false,
                    ApplyTemperatureErrors = applyTemperatureResult?.Errors
                }
            };
        }
        else if (dto is ColorConfigurationDto colorConfiguration)
        {
            ServiceResult<bool>? applyColorResult = null;

            if (colorConfiguration.ApplyColor)
                applyColorResult = _screenService.ApplyColorToScreen(colorConfiguration.Color, colorConfiguration.DevicePath);

            return new ServiceResult<ConfigurationApplyResultDto>()
            {
                Success = true,
                Data = new ColorConfigurationApplyResultDto()
                {
                    DevicePath = colorConfiguration.DevicePath,
                    SucceededToApplyBrightness = applyBrightnessResult?.Success ?? false,
                    ApplyBrightnessErrors = applyBrightnessResult?.Errors,
                    SucceededToApplyColor = applyColorResult?.Success ?? false,
                    ApplyColorErrors = applyColorResult?.Errors
                }
            };
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}