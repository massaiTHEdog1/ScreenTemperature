import { Configuration, ConfigurationDiscriminator } from "./configuration";

export class TemperatureConfiguration extends Configuration {
  public Discriminator = ConfigurationDiscriminator.TemperatureConfiguration;
  public Intensity: number = 6600;

  public constructor(
    model: {
      DevicePath: string;
    } & Partial<TemperatureConfiguration>,
  ) {
    super(model);
  }
}
