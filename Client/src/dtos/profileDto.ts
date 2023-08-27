import { ConfigurationDto } from "./configurations/configurationDto";

export class ProfileDto {
  public Id: string = "";
  public Label: string = "";
  public Configurations: ConfigurationDto[] = [];

  public constructor(dto?: Partial<ProfileDto>) {
    Object.assign(this, dto);
  }
}
