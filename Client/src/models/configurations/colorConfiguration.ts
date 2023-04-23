import { Configuration, ConfigurationDiscriminator } from "./configuration";

export class ColorConfiguration extends Configuration {
    public Discriminator = ConfigurationDiscriminator.ColorConfiguration;
    public Color: string = "#ffffff";

    public constructor(dto?: Partial<ColorConfiguration>) {
        super();

        if (dto)
            Object.assign(this, dto);
    }
}