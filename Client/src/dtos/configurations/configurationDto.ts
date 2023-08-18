export enum ConfigurationDiscriminator {
  TemperatureConfiguration = 0,
  ColorConfiguration = 1,
}

export abstract class ConfigurationDto {
  public abstract Discriminator: ConfigurationDiscriminator;
  public Id: number = 0;
  public DevicePath: string = "";
  public Brightness: number = 100;

  public constructor(dto?: Partial<ConfigurationDto>) {
    Object.assign(this, dto);
  }
}
