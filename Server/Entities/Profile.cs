
using ScreenTemperature.Entities.Configurations;
using ScreenTemperature.Entities.KeyBindingActions;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScreenTemperature.Entities;

public class Profile
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    public string Label { get; set; }

    public List<Configuration> Configurations { get; set; }
    
    /// <summary>
    /// Key bindings
    /// </summary>
    public List<ApplyProfileAction> ApplyProfileActions { get; set; }
}