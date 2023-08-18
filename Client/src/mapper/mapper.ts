import { ColorConfigurationDto } from "../dtos/configurations/colorConfigurationDto";
import { ConfigurationDto } from "../dtos/configurations/configurationDto";
import { TemperatureConfigurationDto } from "../dtos/configurations/temperatureConfigurationDto";
import { ProfileDto } from "../dtos/profileDto";
import { ScreenDto } from "../dtos/screenDto";
import { ColorConfiguration } from "../models/configurations/colorConfiguration";
import {
  Configuration,
  ConfigurationDiscriminator,
} from "../models/configurations/configuration";
import { TemperatureConfiguration } from "../models/configurations/temperatureConfiguration";
import { Profile } from "../models/profile";
import { Screen } from "../models/screen";

export abstract class Mapper {
  // #region Screen

  public static MapScreenToScreenDto(model: Screen): ScreenDto {
    return new ScreenDto({
      DevicePath: model.DevicePath,
      Height: model.Height,
      IsPrimary: model.IsPrimary,
      Label: model.Label,
      Width: model.Width,
      X: model.X,
      Y: model.Y,
    });
  }
  public static MapScreenDtoToScreen(dto: ScreenDto): Screen {
    return new Screen({
      DevicePath: dto.DevicePath,
      Height: dto.Height,
      IsPrimary: dto.IsPrimary,
      Label: dto.Label,
      Width: dto.Width,
      X: dto.X,
      Y: dto.Y,
    });
  }

  // #endregion

  // #region Configurations

  public static MapConfigurationToConfigurationDto(
    model: Configuration,
  ): ConfigurationDto {
    if (model.Discriminator == ConfigurationDiscriminator.ColorConfiguration)
      return this.MapColorConfigurationToColorConfigurationDto(
        model as ColorConfiguration,
      );
    if (
      model.Discriminator == ConfigurationDiscriminator.TemperatureConfiguration
    )
      return this.MapTemperatureConfigurationToTemperatureConfigurationDto(
        model as TemperatureConfiguration,
      );
    else throw new Error("Missing implementation");
  }
  public static MapConfigurationDtoToConfiguration(
    dto: ConfigurationDto,
  ): Configuration {
    if (dto.Discriminator == ConfigurationDiscriminator.ColorConfiguration)
      return this.MapColorConfigurationDtoToColorConfiguration(
        dto as ColorConfigurationDto,
      );
    if (
      dto.Discriminator == ConfigurationDiscriminator.TemperatureConfiguration
    )
      return this.MapTemperatureConfigurationDtoToTemperatureConfiguration(
        dto as TemperatureConfigurationDto,
      );
    else throw new Error("Missing implementation");
  }

  public static MapColorConfigurationToColorConfigurationDto(
    model: ColorConfiguration,
  ): ColorConfigurationDto {
    return new ColorConfigurationDto({
      Color: model.Color,
      DevicePath: model.DevicePath,
      Discriminator: model.Discriminator,
      Id: model.Id,
      Brightness: model.Brightness,
    });
  }
  public static MapColorConfigurationDtoToColorConfiguration(
    dto: ColorConfigurationDto,
  ): ColorConfiguration {
    return new ColorConfiguration({
      Color: dto.Color,
      DevicePath: dto.DevicePath,
      Discriminator: dto.Discriminator,
      Id: dto.Id,
      Brightness: dto.Brightness,
    });
  }

  public static MapTemperatureConfigurationToTemperatureConfigurationDto(
    model: TemperatureConfiguration,
  ): TemperatureConfigurationDto {
    return new TemperatureConfigurationDto({
      DevicePath: model.DevicePath,
      Discriminator: model.Discriminator,
      Id: model.Id,
      Intensity: model.Intensity,
      Brightness: model.Brightness,
    });
  }

  public static MapTemperatureConfigurationDtoToTemperatureConfiguration(
    dto: TemperatureConfigurationDto,
  ): TemperatureConfiguration {
    return new TemperatureConfiguration({
      DevicePath: dto.DevicePath,
      Discriminator: dto.Discriminator,
      Id: dto.Id,
      Intensity: dto.Intensity,
      Brightness: dto.Brightness,
    });
  }

  // #endregion

  // #region Profile

  public static MapProfileToProfileDto(model: Profile): ProfileDto {
    return new ProfileDto({
      Id: model.Id,
      Label: model.Label,
      Configurations:
        model.Configurations?.map((x) =>
          this.MapConfigurationToConfigurationDto(x),
        ) ?? [],
    });
  }
  public static MapProfileDtoToProfile(dto: ProfileDto): Profile {
    return new Profile({
      Id: dto.Id,
      Label: dto.Label,
      Configurations:
        dto.Configurations?.map((x) =>
          this.MapConfigurationDtoToConfiguration(x),
        ) ?? [],
    });
  }

  // #endregion
}
