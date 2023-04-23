import { ConfigurationDto, ConfigurationDiscriminator } from "./configurationDto";

export class ColorConfigurationDto extends ConfigurationDto {
    public Discriminator = ConfigurationDiscriminator.ColorConfiguration;
    public Color : string = "";

    public constructor(dto?: Partial<ColorConfigurationDto>) {
        super();
        
        if(dto)
            Object.assign(this, dto);
    }
}