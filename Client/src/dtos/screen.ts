export class ScreenDto
{
    /** Returns the identifier. */
    public DevicePath : string = "";

    /** Returns the friendly name.*/
    public Label : string = "";

    /** Returns whether this screen Is the primary screen.*/
    public IsPrimary : boolean = false;
    
    /** Returns the X position.*/
    public X : number = 0;

    /** Returns the Y position.*/
    public Y : number = 0;

    /** Returns the width.*/
    public Width : number = 0;

    /** Returns the height.*/
    public Height : number = 0;
}