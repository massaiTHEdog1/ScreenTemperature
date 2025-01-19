export enum ConfigurationDiscriminator {
  TemperatureConfiguration = "temperature",
  ColorConfiguration = "color",
}

export interface ConfigurationDto {
  $type: ConfigurationDiscriminator;
  id: string;
  name: string;
  devicePath: string;
  applyBrightness: boolean;
  brightness: number;
}
