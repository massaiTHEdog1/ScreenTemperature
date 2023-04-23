import { Configuration } from "./configurations/configuration";
import { Screen } from "./screen";

export class Profile {
    /** Returns the identifier. */
    public Id: number = 0;

    /** Returns the friendly name. */
    public Label: string = "";

    /** Returns a {@link Configuration} for each {@link Screen}. */
    public Configurations: Configuration[] = [];

    public constructor(dto?: Partial<Profile>) {
        if (dto)
            Object.assign(this, dto);
    }
}