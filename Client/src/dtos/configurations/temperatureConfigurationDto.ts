import { ConfigurationDto } from "./configurationDto";

export interface TemperatureConfigurationDto extends ConfigurationDto {
  applyIntensity: boolean;
  intensity: number;
}
