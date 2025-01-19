

using Microsoft.EntityFrameworkCore;
using ScreenTemperature.DTOs;
using ScreenTemperature.Entities;
using ScreenTemperature.Mappers;

namespace ScreenTemperature.Services;

public interface IKeyBindingService
{
    Task<ServiceResult<IList<KeyBindingDto>>> ListKeyBindingsAsync(CancellationToken ct);
    Task<ServiceResult<KeyBindingWithHotKeyRegistrationResultDto>> CreateOrUpdateKeyBindingAsync(KeyBindingDto dto, CancellationToken ct);
    Task<ServiceResult> DeleteKeyBindingAsync(Guid id, CancellationToken ct);
}

public class KeyBindingService : IKeyBindingService
{
    private readonly ILogger<KeyBindingService> _logger;
    private readonly DatabaseContext _databaseContext;

    public KeyBindingService(ILogger<KeyBindingService> logger, DatabaseContext databaseContext)
    {
        _logger = logger;
        _databaseContext = databaseContext;
    }

    public async Task<ServiceResult<IList<KeyBindingDto>>> ListKeyBindingsAsync(CancellationToken ct)
    {
        var bindings = await _databaseContext.KeyBindings.Include(binding => binding.Commands).ToListAsync();

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
        var entity = await _databaseContext.KeyBindings.Include(x => x.Commands).FirstOrDefaultAsync(x => x.Id == dto.Id, ct);

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
                await HotKeyManager.UnregisterHotKeyAsync(entity.KeyCode, entity.Alt, entity.Control, entity.Shift);
                shouldRegisterBinding = true;
            }
        }

        entity.KeyCode = dto.KeyCode;
        entity.Shift = dto.Shift;
        entity.Alt = dto.Alt;
        entity.Control = dto.Control;

        #region Configurations

        if (entity.Commands == null) entity.Commands = [];

        var idsCommandsInDto = dto.Commands?.Select(x => x.Id) ?? [];
        var idsCommandsInEntity = entity.Commands!.Select(x => x.Id) ?? [];

        var commandsToCreate = dto.Commands?.Where(x => !idsCommandsInEntity.Contains(x.Id)) ?? [];
        var commandsToDelete = entity.Commands!.Where(x => !idsCommandsInDto.Contains(x.Id)) ?? [];

        // Delete all commands not present in dto
        foreach (var command in commandsToDelete)
        {
            entity.Commands.Remove(command);
        }

        // Create each command not present in entity
        foreach (var command in commandsToCreate)
        {
            throw new NotImplementedException();
        }

        // Update all commands in entity
        foreach (var entityCommand in entity.Commands!)
        {
            var dtoCommand = dto.Commands!.First(x => x.Id == entityCommand.Id);

            throw new NotImplementedException();
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

        await HotKeyManager.UnregisterHotKeyAsync(entity.KeyCode, entity.Alt, entity.Control, entity.Shift);

        return new ServiceResult()
        {
            Success = true
        };
    }
}