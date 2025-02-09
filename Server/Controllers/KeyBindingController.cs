using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScreenTemperature;
using ScreenTemperature.DTOs;
using ScreenTemperature.DTOs.Configurations;
using ScreenTemperature.Entities;
using ScreenTemperature.Mappers;
using System.ComponentModel.DataAnnotations;

[AllowAnonymous]
public class KeyBindingController(ILogger<KeyBindingController> logger, DatabaseContext databaseContext, HotKeyManager hotKeyManager)
{
    [HttpGet("/api/keybindings")]
    public async Task<IResult> GetAllAsync(CancellationToken ct)
    {
        var bindings = await databaseContext.KeyBindings.Include(x => x.Configurations).ToListAsync();

        return TypedResults.Ok(bindings.Select(x => x.ToDto()));
    }



    [HttpPut("/api/keybindings/{id:guid}")]
    public async Task<IResult> CreateOrUpdateAsync([Required] Guid id, [FromBody][Required] KeyBindingDto dto, CancellationToken ct)
    {
        if (id != dto.Id) return TypedResults.BadRequest(new APIErrorResponseDto([$"{nameof(KeyBindingDto.Id)} mismatch."]));

        // todo : Add dto validation

        var shouldRegisterBinding = false;

        // Get entity in database or create a new one
        var entity = await databaseContext.KeyBindings.Include(x => x.Configurations).FirstOrDefaultAsync(x => x.Id == dto.Id, ct);

        // if it is an insertion
        if (entity == null)
        {
            entity = new KeyBinding() { Id = dto.Id };
            databaseContext.KeyBindings.Add(entity);
            shouldRegisterBinding = true;
        }
        // if it is an update
        else
        {
            // if binding is updated
            if (entity.KeyCode != dto.KeyCode || entity.Alt != dto.Alt || entity.Control != dto.Control || !entity.Configurations.Select(x => x.Id).Order().SequenceEqual(dto.ConfigurationIds.Order()))
            {
                await hotKeyManager.UnregisterHotKeyAsync(entity.KeyCode, entity.Alt, entity.Control, false);
                shouldRegisterBinding = true;
            }
        }

        entity.Name = dto.Name;
        entity.KeyCode = dto.KeyCode;
        entity.Alt = dto.Alt;
        entity.Control = dto.Control;
        entity.Configurations = await databaseContext.Configurations.Where(x => dto.ConfigurationIds.Contains(x.Id)).ToListAsync();

        await databaseContext.SaveChangesAsync(ct);

        var isHotKeyRegistered = true;

        if (shouldRegisterBinding)
        {
            isHotKeyRegistered = await hotKeyManager.RegisterHotKeyAsync(dto.KeyCode, dto.Alt, dto.Control, false);
        }

        return TypedResults.Ok(new KeyBindingWithHotKeyRegistrationResultDto()
        {
            IsHotKeyRegistered = isHotKeyRegistered,
            KeyBinding = entity.ToDto()
        });
    }



    [HttpDelete("/api/keybindings/{id:guid}")]
    public async Task<IResult> DeleteAsync([Required] Guid id, CancellationToken ct)
    {
        var entity = await databaseContext.KeyBindings.SingleOrDefaultAsync(x => x.Id == id, cancellationToken: ct);

        if (entity == null) return TypedResults.BadRequest("This key binding does not exist.");

        databaseContext.Remove(entity);

        await databaseContext.SaveChangesAsync(ct);

        await hotKeyManager.UnregisterHotKeyAsync(entity.KeyCode, entity.Alt, entity.Control, false);

        return TypedResults.Ok();
    }
}