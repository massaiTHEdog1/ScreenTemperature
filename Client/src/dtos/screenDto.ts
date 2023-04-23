export class ScreenDto {
    public DevicePath: string = "";
    public Label: string = "";
    public IsPrimary: boolean = false;
    public X: number = 0;
    public Y: number = 0;
    public Width: number = 0;
    public Height: number = 0;

    public constructor(dto?: Partial<ScreenDto>) {
        if(dto)
            Object.assign(this, dto);
    }
}