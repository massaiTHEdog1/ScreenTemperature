import { ProfileDto } from "../dtos/profileDto";
import { TemperatureConfigurationDto } from "../dtos/configurations/temperatureConfigurationDto";
import { Profile } from "../models/profile";
import { Mapper } from "../mapper/mapper";

export class ProfileService {
    private mock: ProfileDto[] = [
        new ProfileDto({
            Id: 1,
            Label: "Normal",
            Configurations: [
                new TemperatureConfigurationDto({
                    Id: 1,
                    DevicePath: "1",
                    Intensity: 6600
                }),
                new TemperatureConfigurationDto({
                    Id: 2,
                    DevicePath: "2",
                    Intensity: 6600
                }),
                new TemperatureConfigurationDto({
                    Id: 3,
                    DevicePath: "3",
                    Intensity: 6600
                }),
            ]
        }),
        new ProfileDto({
            Id: 2,
            Label: "Orange",
            Configurations: [
                new TemperatureConfigurationDto({
                    Id: 4,
                    DevicePath: "1",
                    Intensity: 3309
                }),
                new TemperatureConfigurationDto({
                    Id: 5,
                    DevicePath: "2",
                    Intensity: 3327
                }),
                new TemperatureConfigurationDto({
                    Id: 6,
                    DevicePath: "3",
                    Intensity: 3327
                }),
            ]
        }),
        new ProfileDto({
            Id: 3,
            Label: "Gaming",
            Configurations: [
                new TemperatureConfigurationDto({
                    Id: 7,
                    DevicePath: "1",
                    Intensity: 6600
                }),
                new TemperatureConfigurationDto({
                    Id: 8,
                    DevicePath: "2",
                    Intensity: 3327
                }),
                new TemperatureConfigurationDto({
                    Id: 9,
                    DevicePath: "3",
                    Intensity: 3327
                }),
            ]
        }),
    ];

    public GetConfigurations(): Profile[] {
        return this.mock.map(x => Mapper.MapProfileDtoToProfile(x));
    }
}