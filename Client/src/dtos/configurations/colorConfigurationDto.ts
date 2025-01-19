import { ConfigurationDto } from "./configurationDto";

export interface ColorConfigurationDto extends ConfigurationDto {
  applyColor: boolean;
  color: string;
}
