export enum ConfigurationDiscriminator {
  TemperatureConfiguration = 0,
  ColorConfiguration = 1,
}

export abstract class Configuration {
  public abstract Discriminator: ConfigurationDiscriminator;
  public Id: number = 0;
  public DevicePath: string;
  public Brightness: number = 100;

  public constructor(model: { DevicePath: string & Partial<Configuration> }) {
    this.DevicePath = model.DevicePath;
    Object.assign(this, model);
  }
}
