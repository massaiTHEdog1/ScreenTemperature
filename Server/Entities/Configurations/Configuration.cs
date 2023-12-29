using System.ComponentModel.DataAnnotations.Schema;

namespace ScreenTemperature.Entities.Configurations;

public abstract class Configuration
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    public Profile Profile { get; set; }

    public string DevicePath { get; set; }

    public bool ApplyBrightness { get; set; }
    public byte Brightness { get; set; }
}