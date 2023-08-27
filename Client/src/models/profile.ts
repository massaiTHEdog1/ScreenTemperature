import { Configuration } from "./configurations/configuration";
import { Screen } from "./screen";

export class Profile {
  /** Returns the identifier. */
  public Id: string = "";

  /** Returns the friendly name. */
  public Label: string = "";

  /** Returns a {@link Configuration} for each {@link Screen}. */
  public Configurations: Configuration[] = [];

  public constructor(model?: Partial<Profile>) {
    Object.assign(this, model);
  }
}
