

using Microsoft.EntityFrameworkCore;
using ScreenTemperature.DTOs;
using ScreenTemperature.Entities;
using ScreenTemperature.Mappers;
using ScreenTemperature.Entities.KeyBindingActions;
using ScreenTemperature.DTOs.KeyBindingActions;

namespace ScreenTemperature.Services;

public interface IKeyBindingService
{
    Task<ServiceResult<IList<KeyBindingDto>>> ListKeyBindingsAsync(CancellationToken ct);
    Task<ServiceResult<KeyBindingWithHotKeyRegistrationResultDto>> CreateOrUpdateKeyBindingAsync(KeyBindingDto dto, CancellationToken ct);
    Task<ServiceResult> DeleteKeyBindingAsync(Guid id, CancellationToken ct);
}

public class KeyBindingService : IKeyBindingService
{
    private readonly ILogger<ProfileService> _logger;
    private readonly DatabaseContext _databaseContext;

    public KeyBindingService(ILogger<ProfileService> logger, DatabaseContext databaseContext)
    {
        _logger = logger;
        _databaseContext = databaseContext;
    }

    public async Task<ServiceResult<IList<KeyBindingDto>>> ListKeyBindingsAsync(CancellationToken ct)
    {
        var bindings = await _databaseContext.KeyBindings.Include(binding => binding.Actions).ToListAsync();

        return new ServiceResult<IList<KeyBindingDto>>()
        {
            Success = true,
            Data = bindings.Select(x => x.ToDto()).ToList()
        };
    }

    public async Task<ServiceResult<KeyBindingWithHotKeyRegistrationResultDto>> CreateOrUpdateKeyBindingAsync(KeyBindingDto dto, CancellationToken ct)
    {
        if (dto == null) return new ServiceResult<KeyBindingWithHotKeyRegistrationResultDto>()
        {
            Success = false,
            Errors = ["Invalid parameter."]
        };

        // todo : Add dto validation

        var shouldRegisterBinding = false;

        // Get entity in database or create a new one
        var entity = await _databaseContext.KeyBindings.Include(x => x.Actions).FirstOrDefaultAsync(x => x.Id == dto.Id, ct);

        // if it is an insertion
        if (entity == null)
        {
            entity = new KeyBinding() { Id = dto.Id };
            _databaseContext.KeyBindings.Add(entity);
            shouldRegisterBinding = true;
        }
        // if it is an update
        else
        {
            // if binding is updated
            if(entity.KeyCode != dto.KeyCode || entity.Alt != dto.Alt || entity.Control != dto.Control || entity.Shift != dto.Shift)
            {
                await HotKeyManager.UnregisterHotKeyAsync(entity.KeyCode);
                shouldRegisterBinding = true;
            }
        }

        entity.KeyCode = dto.KeyCode;
        entity.Shift = dto.Shift;
        entity.Alt = dto.Alt;
        entity.Control = dto.Control;

        #region Configurations

        if (entity.Actions == null) entity.Actions = [];

        var idsActionsInDto = dto.Actions?.Select(x => x.Id) ?? [];
        var idsActionsInEntity = entity.Actions!.Select(x => x.Id) ?? [];

        var actionsToCreate = dto.Actions?.Where(x => !idsActionsInEntity.Contains(x.Id)) ?? [];
        var actionsToDelete = entity.Actions!.Where(x => !idsActionsInDto.Contains(x.Id)) ?? [];

        // Delete all actions not present in dto
        foreach (var action in actionsToDelete)
        {
            entity.Actions.Remove(action);
        }

        // Create each action not present in entity
        foreach (var action in actionsToCreate)
        {
            if (action is ApplyProfileActionDto)
            {
                entity.Actions!.Add(new ApplyProfileAction()
                {
                    Id = action.Id,
                });
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        // Update all actions in entity
        foreach (var entityAction in entity.Actions!)
        {
            var dtoAction = dto.Actions!.First(x => x.Id == entityAction.Id);

            if (entityAction is ApplyProfileAction applyProfileAction)
            {
                if (dtoAction is ApplyProfileActionDto applyProfileActionDto)
                {
                    applyProfileAction.ProfileId = applyProfileActionDto.ProfileId;
                }
                else
                {
                    return new ServiceResult<KeyBindingWithHotKeyRegistrationResultDto>()
                    {
                        Success = false,
                        Errors = [$"Cannot change type of action '{dtoAction.Id}'"]
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

        var isHotKeyRegistered = true;

        if (shouldRegisterBinding)
        {
            isHotKeyRegistered = await HotKeyManager.RegisterHotKeyAsync(dto.KeyCode, dto.Alt, dto.Control, dto.Shift);
        }
        

        return new ServiceResult<KeyBindingWithHotKeyRegistrationResultDto>()
        {
            Success = true,
            Data = new KeyBindingWithHotKeyRegistrationResultDto()
            {
                IsHotKeyRegistered = isHotKeyRegistered,
                KeyBinding = entity.ToDto()
            }
        };
    }

    public async Task<ServiceResult> DeleteKeyBindingAsync(Guid id, CancellationToken ct)
    {
        if (id == Guid.Empty) return new ServiceResult()
        {
            Success = false,
            Errors = ["Invalid parameter."]
        };

        var entity = await _databaseContext.KeyBindings.SingleOrDefaultAsync(x => x.Id == id, cancellationToken: ct);

        if (entity == null) return new ServiceResult()
        {
            Success = false,
            Errors = ["This key binding does not exist."]
        };

        _databaseContext.Remove(entity);

        await _databaseContext.SaveChangesAsync(ct);

        await HotKeyManager.UnregisterHotKeyAsync(entity.KeyCode);

        return new ServiceResult()
        {
            Success = true
        };
    }
}