using ScreenTemperature.DTOs.KeyBindingActions;
using ScreenTemperature.Entities.KeyBindingActions;

namespace ScreenTemperature.Mappers
{
    public static class KeyBindingActionMapper
    {
        public static KeyBindingActionDto ToDto(this KeyBindingAction entity)
        {
            if (entity is ApplyProfileAction applyProfileAction)
            {
                return new ApplyProfileActionDto()
                {
                    Id = applyProfileAction.Id,
                    ProfileId = applyProfileAction.ProfileId,
                };
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
