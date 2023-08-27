
using ScreenTemperature.Entities.Configurations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScreenTemperature.Entities;

public class Profile
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    public string Label { get; set; }

    public ICollection<Configuration> Configurations { get; set; }
}