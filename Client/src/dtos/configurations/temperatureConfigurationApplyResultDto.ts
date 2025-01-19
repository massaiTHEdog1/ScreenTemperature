import { ConfigurationApplyResultDto } from "./configurationApplyResultDto";

export interface TemperatureConfigurationApplyResultDto extends ConfigurationApplyResultDto {
  succeededToApplyTemperature: boolean;
  applyTemperatureErrors?: string[];
}
