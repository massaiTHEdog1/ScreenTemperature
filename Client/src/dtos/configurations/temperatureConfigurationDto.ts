import {
  ConfigurationDiscriminator,
  ConfigurationDto,
} from "./configurationDto";

export class TemperatureConfigurationDto extends ConfigurationDto {
  public $type = ConfigurationDiscriminator.TemperatureConfiguration;
  public Intensity: number = 0;

  public constructor(dto?: Partial<TemperatureConfigurationDto>) {
    super();

    Object.assign(this, dto);
  }
}
