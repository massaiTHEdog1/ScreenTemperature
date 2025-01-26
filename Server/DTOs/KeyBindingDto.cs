using ScreenTemperature.DTOs.Commands;

namespace ScreenTemperature.DTOs;

public class KeyBindingDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public IEnumerable<CommandDto> Commands { get; set; }
    public int KeyCode { get; set; }
    public bool Alt { get; set; }
    public bool Shift { get; set; }
    public bool Control { get; set; }
}