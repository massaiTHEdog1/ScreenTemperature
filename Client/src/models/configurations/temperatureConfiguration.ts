import { Configuration, ConfigurationDiscriminator } from "./configuration";

export class TemperatureConfiguration extends Configuration {
    public Discriminator = ConfigurationDiscriminator.TemperatureConfiguration;
    public Intensity: number = 6600;

    public constructor(dto?: Partial<TemperatureConfiguration>) {
        super();

        if (dto)
            Object.assign(this, dto);
    }
}