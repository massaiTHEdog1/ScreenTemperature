using HotChocolate.Execution;
using ScreenTemperature.Entities;
using ScreenTemperature.Services;

namespace ScreenTemperature.Data;

public class Mutation
{
    /// <summary>
    /// Create a <see cref="Configuration"/>.
    /// </summary>
    public async Task<AddConfigurationPayload> AddConfigurationAsync([Service] IConfigurationService configurationRepository, AddConfigurationInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Label))
            throw new ArgumentNullException(nameof(input.Label));

        var configuration = await configurationRepository.AddConfigurationAsync(new Configuration()
        {
            Label = input.Label,
            ScreensConfigurations = input.ScreensConfigurations
        });

        return new AddConfigurationPayload(configuration);
    }
}

public class AddConfigurationInput
{
    public string Label { get; set; }

    public List<ScreenConfiguration> ScreensConfigurations { get; set; }
}


public class AddConfigurationPayload
{
    public AddConfigurationPayload(Configuration configuration)
    {
        Configuration = configuration;
    }

    public Configuration Configuration { get; }
}