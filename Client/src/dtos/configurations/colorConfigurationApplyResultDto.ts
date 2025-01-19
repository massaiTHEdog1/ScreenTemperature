import { ConfigurationApplyResultDto } from "./configurationApplyResultDto";

export interface ColorConfigurationApplyResultDto extends ConfigurationApplyResultDto {
  succeededToApplyColor: boolean;
  applyColorErrors?: string[];
}