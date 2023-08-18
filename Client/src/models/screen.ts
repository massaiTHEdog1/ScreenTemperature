export class Screen {
  /** Returns the identifier. */
  public DevicePath: string = "";

  /** Returns the friendly name. */
  public Label: string = "";

  /** Returns whether this screen Is the primary screen. */
  public IsPrimary: boolean = false;

  /** Returns the X position. */
  public X: number = 0;

  /** Returns the Y position. */
  public Y: number = 0;

  /** Returns the width. */
  public Width: number = 0;

  /** Returns the height. */
  public Height: number = 0;

  /** Returns the width in percentages. */
  public WidthPercentage: number = 0;

  /** Returns the height in percentages. */
  public HeightPercentage: number = 0;

  /** Returns the X position in percentages. */
  public XPercentage: number = 0;

  /** Returns the Y position in percentages. */
  public YPercentage: number = 0;

  /** Returns if the screen is selected. */
  public IsSelected: boolean = false;

  public constructor(model?: Partial<Screen>) {
    Object.assign(this, model);
  }
}
