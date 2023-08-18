import { Configuration, ConfigurationDiscriminator } from "./configuration";

export class ColorConfiguration extends Configuration {
  public Discriminator = ConfigurationDiscriminator.ColorConfiguration;
  public Color: string = "#ffffff";

  public constructor(
    model: {
      DevicePath: string;
    } & Partial<ColorConfiguration>,
  ) {
    super(model);
  }
}
