import { ColorConfigurationApplyResultDto } from "./colorConfigurationApplyResultDto";
import { TemperatureConfigurationApplyResultDto } from "./temperatureConfigurationApplyResultDto";

export enum ConfigurationApplyResultDiscriminator {
  TemperatureConfiguration = "temperature",
  ColorConfiguration = "color",
}

export interface ConfigurationApplyResultDto {
  $type: ConfigurationApplyResultDiscriminator;
  devicePath: string;
  succeededToApplyBrightness: boolean;
  applyBrightnessErrors?: string[];
}

export const isTemperatureConfigurationApplyResult = (config?: ConfigurationApplyResultDto): config is TemperatureConfigurationApplyResultDto =>
  {
    return config?.$type == ConfigurationApplyResultDiscriminator.TemperatureConfiguration;
  };

export const isColorConfigurationApplyResult = (config?: ConfigurationApplyResultDto): config is ColorConfigurationApplyResultDto =>
{
  return config?.$type == ConfigurationApplyResultDiscriminator.ColorConfiguration;
};