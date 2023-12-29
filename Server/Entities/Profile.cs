
using ScreenTemperature.Entities.Configurations;
using ScreenTemperature.Entities.KeyBindingActions;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScreenTemperature.Entities;

public class Profile
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    public string Label { get; set; }

    public ICollection<Configuration> Configurations { get; set; }
    public ICollection<ApplyProfileAction> ApplyProfileActions { get; set; }
}