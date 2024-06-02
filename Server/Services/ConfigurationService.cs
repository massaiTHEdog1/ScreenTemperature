using Microsoft.EntityFrameworkCore;
using ScreenTemperature.DTOs;
using ScreenTemperature.DTOs.Configurations;
using ScreenTemperature.Entities.Configurations;
using ScreenTemperature.Mappers;

namespace ScreenTemperature.Services;

public interface IConfigurationService
{
    Task<ServiceResult<IList<ConfigurationDto>>> GetAllAsync();
    Task<ServiceResult<ConfigurationDto>> CreateOrUpdateAsync(ConfigurationDto dto);
    Task<ServiceResult> DeleteAsync(Guid id);
}

public class ConfigurationService : IConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;
    private readonly DatabaseContext _databaseContext;

    public ConfigurationService(ILogger<ConfigurationService> logger, DatabaseContext databaseContext)
    {
        _logger = logger;
        _databaseContext = databaseContext;
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

    public async Task<ServiceResult<ConfigurationDto>> CreateAsync(ConfigurationDto dto)
    {
        if (dto == null) return new ServiceResult<ConfigurationDto>()
        {
            Success = false,
            Errors = ["Invalid parameter."]
        };

        // todo : Add dto validation

        var entity = await _databaseContext.Configurations.FirstOrDefaultAsync(x => x.Id == dto.Id);

        if (entity != null) return new ServiceResult<ConfigurationDto>()
        {
            Success = false,
            Errors = ["This id already exists."]
        };

        

        _databaseContext.Configurations.Add(entity);

        await _databaseContext.SaveChangesAsync();

        return new ServiceResult<ConfigurationDto>()
        {
            Success = true,
            Data = entity.ToDto(),
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
        
        var entity = await _databaseContext.Configurations.Include(x => x.Profiles).SingleOrDefaultAsync(x => x.Id == id);

        if(entity == null) return new ServiceResult()
        {
            Success = false,
            Errors = ["This configuration does not exist."]
        };

        // Cannot delete if this configuration is used in a profile
        if (entity.Profiles?.Count > 0) return new ServiceResult()
        {
            Success = false,
            Errors = ["This profile is used in at least one profile."]
        };

        _databaseContext.Configurations.Remove(entity);

        await _databaseContext.SaveChangesAsync();

        return new ServiceResult()
        {
            Success = true
        };
    }
}