using Microsoft.EntityFrameworkCore;
using ScreenTemperature.DTOs;
using ScreenTemperature.DTOs.Configurations;
using ScreenTemperature.Entities.Configurations;
using ScreenTemperature.Mappers;

namespace ScreenTemperature.Services;

public interface IProfileService
{
    Task<IList<ProfileDto>> ListProfilesAsync(CancellationToken ct);

    Task<ProfileDto> AddProfileAsync(ProfileDto dto, CancellationToken ct);

    Task<ProfileDto> UpdateProfileAsync(ProfileDto dto, CancellationToken ct);

    Task DeleteProfileAsync(Guid profileId, CancellationToken ct);
}

public class ProfileService : IProfileService
{
    private readonly ILogger<ProfileService> _logger;
    private readonly DatabaseContext _databaseContext;

    public ProfileService(ILogger<ProfileService> logger, DatabaseContext databaseContext)
    {
        _logger = logger;
        _databaseContext = databaseContext;
    }

    public async Task<IList<ProfileDto>> ListProfilesAsync(CancellationToken ct)
    {
        var profiles = await _databaseContext.Profiles.Include(profile => profile.Configurations).ToListAsync();
        return profiles.Select(x => x.ToDto()).ToList();
    }

    public async Task<ProfileDto> AddProfileAsync(ProfileDto dto, CancellationToken ct)
    {
        if(dto == null) throw new ArgumentNullException(nameof(dto));

        var entity = dto.ToEntity();

        _databaseContext.Profiles.Add(entity);
        await _databaseContext.SaveChangesAsync(ct);

        return entity.ToDto();
    }

    public async Task<ProfileDto> UpdateProfileAsync(ProfileDto dto, CancellationToken ct)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var entity = await _databaseContext.Profiles.Include(x => x.Configurations).FirstOrDefaultAsync(x => x.Id == dto.Id, ct);

        if (entity == null) return null;

        entity.Label = dto.Label;

        #region Configurations

        // Make a copy of configurations in the dto
        var configurationsToCreate = new List<ConfigurationDto>(dto.Configurations);

        foreach (var configurationToUpdate in entity.Configurations)
        {
            var configurationInDto = dto.Configurations?.FirstOrDefault(x => x.Id == configurationToUpdate.Id);

            // If this configuration is not present in the dto
            if (configurationInDto == null)
            {
                // delete it
                _databaseContext.Remove(configurationToUpdate);
                continue;
            }

            // This configuration is updated so we remove it from the creation list
            configurationsToCreate.Remove(configurationInDto);

            configurationToUpdate.Brightness = configurationInDto.Brightness;
            configurationToUpdate.DevicePath = configurationInDto.DevicePath;

            if (configurationToUpdate is TemperatureConfiguration temperatureConfiguration)
            {
                if(configurationInDto is TemperatureConfigurationDto temperatureConfigurationDto)
                {
                    temperatureConfiguration.Intensity = temperatureConfigurationDto.Intensity;
                }
            }
            else if(configurationToUpdate is ColorConfiguration colorConfiguration)
            {
                if (configurationInDto is ColorConfigurationDto colorConfigurationDto)
                {
                    colorConfiguration.Color = colorConfigurationDto.Color;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        foreach (var configurationToCreate in configurationsToCreate)
        {
            entity.Configurations.Add(configurationToCreate.ToEntity());
        }

        #endregion

        await _databaseContext.SaveChangesAsync(ct);

        return entity.ToDto();
    }

    public async Task DeleteProfileAsync(Guid profileId, CancellationToken ct)
    {
        if (profileId == Guid.Empty) return;

        var entity = await _databaseContext.Profiles.FindAsync(new object[] { profileId }, cancellationToken: ct);

        if(entity == null) return;

        _databaseContext.Remove(entity);

        await _databaseContext.SaveChangesAsync(ct);
    }
}