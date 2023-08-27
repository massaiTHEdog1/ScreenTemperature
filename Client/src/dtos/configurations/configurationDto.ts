export enum ConfigurationDiscriminator {
  TemperatureConfiguration = 0,
  ColorConfiguration = 1,
}

export abstract class ConfigurationDto {
  public abstract $type: ConfigurationDiscriminator;
  public Id: string = "";
  public DevicePath: string = "";
  public Brightness: number = 100;

  public constructor(dto?: Partial<ConfigurationDto>) {
    Object.assign(this, dto);
  }
}
