using Microsoft.EntityFrameworkCore;
using ScreenTemperature.DTOs;
using ScreenTemperature.DTOs.Configurations;
using ScreenTemperature.Entities;
using ScreenTemperature.Entities.Configurations;
using ScreenTemperature.Mappers;

namespace ScreenTemperature.Services;

public interface IProfileService
{
    Task<ServiceResult<IList<ProfileDto>>> ListProfilesAsync(CancellationToken ct);
    Task<ServiceResult<IList<ConfigurationApplyResultDto>>> ApplyProfileAsync(Guid id, Guid[]? configurationIds, CancellationToken ct);
    Task<ServiceResult<ProfileDto>> CreateOrUpdateProfileAsync(ProfileDto dto, CancellationToken ct);
    Task<ServiceResult> DeleteProfileAsync(Guid id, CancellationToken ct);
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

    public async Task<ServiceResult<IList<ProfileDto>>> ListProfilesAsync(CancellationToken ct)
    {
        var profiles = await _databaseContext.Profiles.Include(profile => profile.Configurations).ToListAsync();

        return new ServiceResult<IList<ProfileDto>>()
        {
            Success = true,
            Data = profiles.Select(x => x.ToDto()).ToList()
        };
    }

    public async Task<ServiceResult<ProfileDto>> CreateOrUpdateProfileAsync(ProfileDto dto, CancellationToken ct)
    {
        if (dto == null) return new ServiceResult<ProfileDto>()
        {
            Success = false,
            Errors = ["Invalid parameter."]
        };

        // todo : Add dto validation

        // Get entity in database or create a new one
        var entity = await _databaseContext.Profiles.Include(x => x.Configurations).FirstOrDefaultAsync(x => x.Id == dto.Id, ct);

        if (entity == null)
        {
            entity = new Profile() { Id = dto.Id };
            _databaseContext.Profiles.Add(entity);
        }

        entity.Label = dto.Label;

        #region Configurations

        if (entity.Configurations == null) entity.Configurations = [];

        var idsConfigurationsInDto = dto.Configurations?.Select(x => x.Id) ?? [];
        var idsConfigurationsInEntity = entity.Configurations!.Select(x => x.Id) ?? [];

        var configurationsToCreate = dto.Configurations?.Where(x => !idsConfigurationsInEntity.Contains(x.Id)) ?? [];
        var configurationsToDelete = entity.Configurations!.Where(x => !idsConfigurationsInDto.Contains(x.Id)) ?? [];

        // Delete all configurations not present in dto
        foreach (var configuration in configurationsToDelete)
        {
            entity.Configurations.Remove(configuration);
        }

        // Create each configuration not present in entity
        foreach (var configuration in configurationsToCreate)
        {
            if(configuration is TemperatureConfigurationDto)
            {
                entity.Configurations!.Add(new TemperatureConfiguration()
                {
                    Id = configuration.Id,
                });
            }
            else if(configuration is ColorConfigurationDto)
            {
                entity.Configurations!.Add(new ColorConfiguration()
                {
                    Id = configuration.Id,
                });
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        // Update all configurations in entity
        foreach (var entityConfiguration in entity.Configurations!)
        {
            var dtoConfiguration = dto.Configurations!.First(x => x.Id == entityConfiguration.Id);

            entityConfiguration.ApplyBrightness = dtoConfiguration.ApplyBrightness;
            entityConfiguration.Brightness = dtoConfiguration.Brightness;
            entityConfiguration.DevicePath = dtoConfiguration.DevicePath;

            if (entityConfiguration is TemperatureConfiguration temperatureConfiguration)
            {
                if (dtoConfiguration is TemperatureConfigurationDto temperatureConfigurationDto)
                {
                    temperatureConfiguration.ApplyIntensity = temperatureConfigurationDto.ApplyIntensity;
                    temperatureConfiguration.Intensity = temperatureConfigurationDto.Intensity;
                }
                else
                {
                    return new ServiceResult<ProfileDto>()
                    {
                        Success = false,
                        Errors = [$"Cannot change type of configuration '{dtoConfiguration.Id}'"]
                    };
                }
            }
            else if (entityConfiguration is ColorConfiguration colorConfiguration)
            {
                if (dtoConfiguration is ColorConfigurationDto colorConfigurationDto)
                {
                    colorConfiguration.ApplyColor = colorConfigurationDto.ApplyColor;
                    colorConfiguration.Color = colorConfigurationDto.Color;
                }
                else
                {
                    return new ServiceResult<ProfileDto>()
                    {
                        Success = false,
                        Errors = [$"Cannot change type of configuration '{dtoConfiguration.Id}'"]
                    };
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        await _databaseContext.SaveChangesAsync(ct);

        return new ServiceResult<ProfileDto>()
        {
            Success = true,
            Data = entity.ToDto(),
        };
    }

    public async Task<ServiceResult> DeleteProfileAsync(Guid id, CancellationToken ct)
    {
        if (id == Guid.Empty) return new ServiceResult() 
        {
            Success = false,
            Errors = ["Invalid parameter."]
        };

        var entity = await _databaseContext.Profiles.Include(x => x.ApplyProfileActions).SingleOrDefaultAsync(x => x.Id == id, cancellationToken: ct);

        if(entity == null) return new ServiceResult()
        {
            Success = false,
            Errors = ["This profile does not exist."]
        };

        // Cannot delete if this profile is used in key bindings
        if(entity.ApplyProfileActions?.Count() > 0) return new ServiceResult()
        {
            Success = false,
            Errors = ["This profile is used in key bindings."]
        };

        _databaseContext.Remove(entity);

        await _databaseContext.SaveChangesAsync(ct);

        return new ServiceResult()
        {
            Success = true
        };
    }

    public async Task<ServiceResult<IList<ConfigurationApplyResultDto>>> ApplyProfileAsync(Guid id, Guid[]? configurationIds, CancellationToken ct)
    {
        if (id == Guid.Empty) return new ServiceResult<IList<ConfigurationApplyResultDto>>()
        {
            Success = false,
            Errors = ["Invalid parameter."]
        };

        // Get profile
        var profile = await _databaseContext.Profiles.Include(profile => profile.Configurations).SingleOrDefaultAsync(profile => profile.Id == id, ct);

        // if profile does not exist
        if (profile == null) return new ServiceResult<IList<ConfigurationApplyResultDto>>()
        {
            Success = false,
            Errors = ["This profile does not exist."]
        };

        // List of configurations to apply
        IList<Configuration> configurationsToApply = new List<Configuration>();
        
        // If a list is in parameters
        if(configurationIds?.Length > 0)
        {
            foreach (var configurationId in configurationIds ?? [])
            {
                var configuration = profile.Configurations?.FirstOrDefault(x => x.Id == configurationId);

                if (configuration == null) return new ServiceResult<IList<ConfigurationApplyResultDto>>()
                {
                    Success = false,
                    Errors = [$"Configuration '{configurationId}' does not exist on profile '{id}'."]
                };

                configurationsToApply.Add(configuration);
            }
        }
        // Apply all configurations
        else
        {
            configurationsToApply = profile.Configurations?.ToList() ?? [];
        }

        var results = new List<ConfigurationApplyResultDto>();

        foreach (var configuration in configurationsToApply)
        {
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