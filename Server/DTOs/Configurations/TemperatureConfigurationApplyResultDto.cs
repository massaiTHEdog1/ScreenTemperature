namespace ScreenTemperature.DTOs.Configurations
{
    public class TemperatureConfigurationApplyResultDto : ConfigurationApplyResultDto
    {
        public bool SucceededToApplyTemperature { get; set; }
        public string[] ApplyTemperatureErrors { get; set; }
    }
}
