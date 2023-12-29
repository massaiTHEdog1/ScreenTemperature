using ScreenTemperature.DTOs.KeyBindingActions;

namespace ScreenTemperature.DTOs;

public class KeyBindingDto
{
    public Guid Id { get; set; }
    public IEnumerable<KeyBindingActionDto> Actions { get; set; }
    public int KeyCode { get; set; }
    public bool Alt { get; set; }
    public bool Shift { get; set; }
    public bool Control { get; set; }
}