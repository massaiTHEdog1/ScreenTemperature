
namespace ScreenTemperature.Entities.KeyBindingActions;

public class ApplyProfileAction : KeyBindingAction
{
    public Profile Profile { get; set; }
    public Guid ProfileId { get; set; }
}