using System.ComponentModel.DataAnnotations.Schema;

namespace ScreenTemperature.Entities.Configurations;

public abstract class Configuration
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }
    public string Name { get; set; }

    public string DevicePath { get; set; }

    public bool ApplyBrightness { get; set; }
    public byte Brightness { get; set; }

    public List<KeyBinding> KeyBindings { get; set; } = [];

    public bool ApplyAtStartup { get; set; }
}