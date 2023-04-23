import { ConfigurationDto, ConfigurationDiscriminator } from "./configurationDto";

export class TemperatureConfigurationDto extends ConfigurationDto {
    public Discriminator = ConfigurationDiscriminator.TemperatureConfiguration;
    public Intensity : number = 0;

    public constructor(dto?: Partial<TemperatureConfigurationDto>) {
        super();
        
        if(dto)
            Object.assign(this, dto);
    }
}