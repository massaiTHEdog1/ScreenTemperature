using System.ComponentModel.DataAnnotations.Schema;
using ScreenTemperature.Entities.Configurations;

namespace ScreenTemperature.Entities.Commands;

public class ApplyConfigurationCommand : Command
{
    /// <summary>
    /// Returns the identifier of the associated <see cref="Configurations.Configuration"/>.
    /// </summary>
    public Guid ConfigurationId { get; set; }

    /// <summary>
    /// Returns the associated <see cref="Configurations.Configuration"/>.
    /// </summary>
    public Configuration Configuration { get; set; }
}