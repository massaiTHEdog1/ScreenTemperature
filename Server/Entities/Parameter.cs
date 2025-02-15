
using ScreenTemperature.Entities.Configurations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScreenTemperature.Entities;

public class Parameter
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Key { get; set; }
    public string Value { get; set; }
}