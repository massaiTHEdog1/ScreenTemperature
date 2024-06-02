using Microsoft.EntityFrameworkCore;
using ScreenTemperature.DTOs;
using ScreenTemperature.DTOs.Configurations;
using ScreenTemperature.Entities;
using ScreenTemperature.Entities.Configurations;
using ScreenTemperature.Mappers;

namespace ScreenTemperature.Services;

public interface IProfileService
{
    Task<ServiceResult<IList<ProfileDto>>> GetAllAsync();
    Task<ServiceResult<ProfileDto>> CreateOrUpdateAsync(ProfileDto dto);
    Task<ServiceResult> DeleteAsync(Guid id);
    Task<ServiceResult<IList<ConfigurationApplyResultDto>>> ApplyAsync(Guid id);
}

public class ProfileService : IProfileService
{
    private readonly ILogger<ProfileService> _logger;
    private readonly DatabaseContext _databaseContext;
    private readonly IScreenService _screenService;

    public ProfileService(ILogger<ProfileService> logger, DatabaseContext databaseContext, IScreenService screenService)
    {
        _logger = logger;
        _databaseContext = databaseContext;
        _screenService = screenService;
    }

    public async Task<ServiceResult<IList<ProfileDto>>> GetAllAsync()
    {
        var profiles = await _databaseContext.Profiles.Include(profile => profile.Configurations).ToListAsync();

        return new ServiceResult<IList<ProfileDto>>()
        {
            Success = true,
            Data = profiles.Select(x => x.ToDto()).ToList()
        };
    }

    public async Task<ServiceResult<ProfileDto>> CreateOrUpdateAsync(ProfileDto dto)
    {
        if (dto == null) return new ServiceResult<ProfileDto>()
        {
            Success = false,
            Errors = ["Invalid parameter."]
        };

        // todo : Add dto validation

        var entity = await _databaseContext.Profiles.Include(x => x.Configurations).Include(x => x.ApplyProfileActions).FirstOrDefaultAsync(x => x.Id == dto.Id);

        if (entity == null)
        {
            entity = new Profile() { Id = dto.Id };
            _databaseContext.Profiles.Add(entity);
        }

        entity.Label = dto.Label;

        if (entity.Configurations == null)
            entity.Configurations = [];
        else // remove configurations which are not in dto
            entity.Configurations.RemoveAll(conf => !dto.Configurations?.Any(id => conf.Id == id) ?? true);

        foreach (var configurationId in dto.Configurations ?? [])
        {
            var configuration = await _databaseContext.Configurations.FirstOrDefaultAsync(x => x.Id == configurationId);

            if (configuration == null) return new ServiceResult<ProfileDto>()
            {
                Success = false,
                Errors = [$"Configuration with id {configurationId} does not exist."]
            };

            if(!entity.Configurations.Any(x => x.Id == configurationId))
                entity.Configurations.Add(configuration);
        }

        await _databaseContext.SaveChangesAsync();

        return new ServiceResult<ProfileDto>()
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

        var entity = await _databaseContext.Profiles.SingleOrDefaultAsync(x => x.Id == id);

        if(entity == null) return new ServiceResult()
        {
            Success = false,
            Errors = ["This profile does not exist."]
        };

        // Cannot delete if this profile is used in key bindings
        if (entity.ApplyProfileActions?.Count() > 0) return new ServiceResult()
        {
            Success = false,
            Errors = ["This profile is used in at least one key binding."]
        };

        _databaseContext.Remove(entity);

        await _databaseContext.SaveChangesAsync();

        return new ServiceResult()
        {
            Success = true
        };
    }

    public async Task<ServiceResult<IList<ConfigurationApplyResultDto>>> ApplyAsync(Guid id)
    {
        if (id == Guid.Empty) return new ServiceResult<IList<ConfigurationApplyResultDto>>()
        {
            Success = false,
            Errors = ["Invalid parameter."]
        };

        // Get profile
        var profile = await _databaseContext.Profiles.Include(profile => profile.Configurations).SingleOrDefaultAsync(profile => profile.Id == id);

        // if profile does not exist
        if (profile == null) return new ServiceResult<IList<ConfigurationApplyResultDto>>()
        {
            Success = false,
            Errors = ["This profile does not exist."]
        };

        // List of configurations to apply
        var configurationsToApply = profile.Configurations?.ToList() ?? [];

        var results = new List<ConfigurationApplyResultDto>();

        foreach (var configuration in configurationsToApply)
        {
            if(configuration.ApplyBrightness)
            {
                _screenService.ApplyBrightnessToScreen(configuration.Brightness, configuration.DevicePath);
            }

            if (configuration is TemperatureConfiguration temperatureConfiguration)
            {
                if (!temperatureConfiguration.ApplyIntensity) continue;

                var result = _screenService.ApplyKelvinToScreen(temperatureConfiguration.Intensity, temperatureConfiguration.DevicePath);

                if (!result.Success) return new ServiceResult<IList<ConfigurationApplyResultDto>>()
                {
                    Success = false,
                    Errors = result.Errors
                };

                results.Add(new ConfigurationApplyResultDto()
                {
                    DevicePath = temperatureConfiguration.DevicePath,
                    Succeeded = result.Data,
                });
            }
            else if (configuration is ColorConfiguration colorConfiguration)
            {
                if (!colorConfiguration.ApplyColor) continue;

                var result = _screenService.ApplyColorToScreen(colorConfiguration.Color, colorConfiguration.DevicePath);

                if (!result.Success) return new ServiceResult<IList<ConfigurationApplyResultDto>>()
                {
                    Success = false,
                    Errors = result.Errors
                };

                results.Add(new ConfigurationApplyResultDto()
                {
                    DevicePath = colorConfiguration.DevicePath,
                    Succeeded = result.Data,
                });
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        return new ServiceResult<IList<ConfigurationApplyResultDto>>()
        {
            Success = true,
            Data = results
        };
    }
}