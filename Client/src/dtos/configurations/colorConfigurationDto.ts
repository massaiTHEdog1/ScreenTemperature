import {
  ConfigurationDiscriminator,
  ConfigurationDto,
} from "./configurationDto";

export class ColorConfigurationDto extends ConfigurationDto {
  public $type = ConfigurationDiscriminator.ColorConfiguration;
  public Color: string = "";

  public constructor(dto?: Partial<ColorConfigurationDto>) {
    super();

    Object.assign(this, dto);
  }
}
