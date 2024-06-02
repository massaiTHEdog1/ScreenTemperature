using System.ComponentModel.DataAnnotations.Schema;

namespace ScreenTemperature.Entities.Configurations;

public abstract class Configuration
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    public List<Profile> Profiles { get; set; }

    public string DevicePath { get; set; }

    public bool ApplyBrightness { get; set; }
    public byte Brightness { get; set; }
}