import { ScreenDto } from "../dtos/screen";

export class ScreenService
{
    public GetScreens() : ScreenDto[]
    {
        return [
            {
                DevicePath: "1",
                Width: 2560,
                Height: 1440,
                IsPrimary: true,
                Label: "ROG PG278Q",
                X: 0,
                Y: 0,
            },
            {
                DevicePath: "2",
                Width: 1920,
                Height: 1080,
                IsPrimary: false,
                Label: "VG248",
                X: 2560,
                Y: 0,
            },
            {
                DevicePath: "3",
                Width: 3840,
                Height: 2160,
                IsPrimary: true,
                Label: "SAMSUNG",
                X: -3840,
                Y: 0,
            },
        ];
    }
}