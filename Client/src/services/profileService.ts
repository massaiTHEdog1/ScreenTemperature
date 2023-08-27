import axios from "axios";
import { ProfileDto } from "../dtos/profileDto";
import { Mapper } from "../mapper/mapper";
import { Profile } from "../models/profile";

export default new (class ProfileService {
  async ListProfilesAsync(): Promise<Profile[]> {
    const response = await axios.get<ProfileDto[]>(
      `${import.meta.env.VITE_SERVER_BASE_URL}/api/profiles/list`,
    );

    return response.data.map((x) => Mapper.MapProfileDtoToProfile(x));
  }

  async DeleteProfileAsync(Id: string): Promise<boolean> {
    await new Promise((f) => setTimeout(f, 1000));

    return true;
  }

  async SaveProfileAsync(profile: Profile): Promise<Profile> {
    await new Promise((f) => setTimeout(f, 1000));

    return new Profile();
  }
})();
