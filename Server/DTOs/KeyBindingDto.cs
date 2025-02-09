namespace ScreenTemperature.DTOs;

public class KeyBindingDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<Guid> ConfigurationIds { get; set; } = [];
    public int KeyCode { get; set; }
    public bool Alt { get; set; }
    public bool Control { get; set; }
}