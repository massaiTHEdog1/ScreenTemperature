export enum ConfigurationDiscriminator {
    TemperatureConfiguration = 0,
    ColorConfiguration = 1,
}

export abstract class ConfigurationDto{
    public abstract Discriminator: ConfigurationDiscriminator;
    public Id: number = 0;
    public DevicePath: string = "";

    public constructor(dto?: Partial<ConfigurationDto>) {        
        if(dto)
            Object.assign(this, dto);
    }
}