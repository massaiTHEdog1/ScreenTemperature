export enum ConfigurationDiscriminator {
    TemperatureConfiguration = 0,
    ColorConfiguration = 1,
}

export abstract class Configuration {
    public abstract Discriminator: ConfigurationDiscriminator;
    public Id: number = 0;
    public DevicePath: string = "";

    public constructor(dto?: Partial<Configuration>) {
        if (dto)
            Object.assign(this, dto);
    }
}