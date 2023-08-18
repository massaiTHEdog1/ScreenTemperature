import { TemperatureConfigurationDto } from "../dtos/configurations/temperatureConfigurationDto";
import { ProfileDto } from "../dtos/profileDto";
import { Mapper } from "../mapper/mapper";
import { Profile } from "../models/profile";

export default new (class ProfileService {
  private mock: ProfileDto[] = [
    new ProfileDto({
      Id: 1,
      Label: "Normal",
      Configurations: [
        new TemperatureConfigurationDto({
          Id: 1,
          DevicePath: "1",
          Intensity: 6600,
        }),
        new TemperatureConfigurationDto({
          Id: 2,
          DevicePath: "2",
          Intensity: 6600,
        }),
        new TemperatureConfigurationDto({
          Id: 3,
          DevicePath: "3",
          Intensity: 6600,
        }),
      ],
    }),
    new ProfileDto({
      Id: 2,
      Label: "Orange",
      Configurations: [
        new TemperatureConfigurationDto({
          Id: 4,
          DevicePath: "1",
          Intensity: 3309,
        }),
        new TemperatureConfigurationDto({
          Id: 5,
          DevicePath: "2",
          Intensity: 3327,
        }),
        new TemperatureConfigurationDto({
          Id: 6,
          DevicePath: "3",
          Intensity: 3327,
        }),
      ],
    }),
    new ProfileDto({
      Id: 3,
      Label: "Gaming",
      Configurations: [
        new TemperatureConfigurationDto({
          Id: 7,
          DevicePath: "1",
          Intensity: 6600,
        }),
        new TemperatureConfigurationDto({
          Id: 8,
          DevicePath: "2",
          Intensity: 3327,
        }),
        new TemperatureConfigurationDto({
          Id: 9,
          DevicePath: "3",
          Intensity: 3327,
        }),
      ],
    }),
  ];

  async GetProfilesAsync(): Promise<Profile[]> {
    await new Promise((f) => setTimeout(f, 1000));
    return this.mock.map((x) => Mapper.MapProfileDtoToProfile(x));
  }

  async DeleteProfileAsync(Id: number): Promise<boolean> {
    await new Promise((f) => setTimeout(f, 1000));
    this.mock = this.mock.filter((x) => x.Id != Id);

    return true;
  }

  async SaveProfileAsync(profile: Profile): Promise<Profile> {
    await new Promise((f) => setTimeout(f, 1000));

    const copy = JSON.parse(JSON.stringify(profile)) as Profile;

    if (copy.Id == 0) {
      // Generate an incremented id
      copy.Id = this.mock.reduce(
        (accumulator, currentValue) =>
          currentValue.Id > accumulator - 1 ? currentValue.Id : accumulator,
        0,
      );

      let configIndex = 1;

      copy.Configurations?.forEach((x) => {
        x.Id = configIndex;
        configIndex++;
      });

      const newLength = this.mock.push(Mapper.MapProfileToProfileDto(copy));

      return Mapper.MapProfileDtoToProfile(this.mock[newLength - 1]);
    } else {
      const dto = this.mock.find((x) => x.Id == copy.Id)!;

      let configIndex =
        dto.Configurations?.reduce(
          (accumulator, currentValue) =>
            currentValue.Id > accumulator - 1 ? currentValue.Id : accumulator,
          1,
        ) ?? 1;

      copy.Configurations?.forEach((x) => {
        x.Id = x.Id == 0 ? configIndex : x.Id;
        configIndex++;
      });

      const dtoIndex = this.mock.indexOf(dto);

      this.mock[dtoIndex] = Mapper.MapProfileToProfileDto(copy);

      return Mapper.MapProfileDtoToProfile(this.mock[dtoIndex]);
    }
  }
})();
