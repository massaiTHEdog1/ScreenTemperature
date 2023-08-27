<script lang="ts">
import { defineComponent, ref } from "vue";
import { ColorConfiguration } from "../models/configurations/colorConfiguration";
import {
  Configuration,
  ConfigurationDiscriminator,
} from "../models/configurations/configuration";
import { TemperatureConfiguration } from "../models/configurations/temperatureConfiguration";
import { Profile } from "../models/profile";
import { Screen } from "../models/screen";
import {
  default as ProfileService,
  default as profileService,
} from "../services/profileService";
import ScreenService from "../services/screenService";
import ScreensViewer from "./ScreensViewer.vue";

export default defineComponent({
  components: {
    ScreensViewer,
  },
  data() {
    return {
      screens: ref<Screen[]>([]),
      profiles: ref<Profile[]>([]),

      /** The displayed {@link Profile} */
      currentProfile: ref<Profile>(),

      form: ref<{
        configurationType?: ConfigurationDiscriminator;
        temperatureIntensity?: number;
        color?: string;
        brightness?: number;
      }>({}),

      isLoading: ref<boolean>(true),

      isEditingProfileLabel: ref<boolean>(false),

      profileLabelBeforeEdit: "",

      ConfigurationDiscriminator: ConfigurationDiscriminator,

      selectedScreens: ref<Screen[]>([]),

      showBrightness: ref<boolean>(false),
    };
  },
  computed: {
    isButtonSaveDisabled() {
      return (
        !this.currentProfile ||
        !this.isProfileNewOrHasChanged(this.currentProfile, this.profiles)
      );
    },
    isButtonDuplicateDisabled() {
      return (
        !this.currentProfile ||
        this.isProfileNewOrHasChanged(this.currentProfile, this.profiles)
      );
    },
  },
  async mounted() {
    this.isLoading = true;

    // Load the screens and the profiles
    this.screens = ScreenService.GetScreens().map((x) => new Screen(x));
    this.profiles = await ProfileService.ListProfilesAsync();

    // If there is at least one saved profile
    if (this.profiles.length > 0) {
      // Show the first profile
      this.currentProfile = JSON.parse(JSON.stringify(this.profiles[0]));
    }

    this.resetForm();

    this.isLoading = false;
  },
  methods: {
    /** Returns true if the profile has changed. */
    isProfileNewOrHasChanged(profile: Profile, profiles: Profile[]) {
      const originalProfile = profiles.find((x) => x.Id == profile.Id);
      return (
        profile.Id == "" ||
        JSON.stringify(profile) != JSON.stringify(originalProfile)
      );
    },
    /** Get the configuration associated to the screen, if there is any. */
    getScreenConfiguration(screen: Screen, configurations: Configuration[]) {
      return configurations.find((x) => x.DevicePath == screen.DevicePath);
    },
    resetForm() {
      this.showBrightness = true;

      // For each selected screen
      for (let i = 0; i < this.selectedScreens.length; i++) {
        // Get the configuration of the screen (can be null)
        const associatedConfiguration = this.getScreenConfiguration(
          this.selectedScreens[i],
          this.currentProfile?.Configurations ?? [],
        );

        // Hide brightness if a screen has no configuration
        if (!associatedConfiguration) this.showBrightness = false;

        // If this is the first screen, initialize the form
        if (i == 0) {
          this.form = {
            configurationType: associatedConfiguration?.Discriminator,
            temperatureIntensity: (
              associatedConfiguration as TemperatureConfiguration
            )?.Intensity,
            color: (associatedConfiguration as ColorConfiguration)?.Color,
            brightness: associatedConfiguration?.Brightness,
          };
        } else {
          // If the type of this configuration is the same than the previous one
          this.form.configurationType =
            this.form.configurationType ==
            associatedConfiguration?.Discriminator
              ? this.form.configurationType
              : undefined;

          // If the temperature of this configuration is the same than the previous one
          this.form.temperatureIntensity =
            this.form.temperatureIntensity ==
            (associatedConfiguration as TemperatureConfiguration)?.Intensity
              ? this.form.temperatureIntensity
              : undefined;

          // If the color of this configuration is the same than the previous one
          this.form.color =
            this.form.color ==
            (associatedConfiguration as ColorConfiguration)?.Color
              ? this.form.color
              : undefined;

          // If the brightness of this configuration is the same than the previous one
          this.form.brightness =
            this.form.brightness == associatedConfiguration?.Brightness
              ? this.form.brightness
              : undefined;
        }
      }
    },
    onScreenSelectionChanged(selectedScreens: Screen[]) {
      this.selectedScreens = selectedScreens;
      this.resetForm();
    },
    /** Display a modal asking for confirmation before switching profile. */
    askConfirmationBeforeSwitchingProfile(): boolean {
      if (!this.currentProfile) return true;

      // If the current profile is a new one or it has not been saved
      if (this.isProfileNewOrHasChanged(this.currentProfile, this.profiles)) {
        const confirmation = confirm(
          "You have not saved this profile. You will lose all your changes. \nAre you sure ?",
        );

        // If this profile is a new one
        if (this.currentProfile.Id == "" && confirmation) {
          // Delete the profile from the list
          this.profiles.splice(this.profiles.length - 1, 1);
        }

        return confirmation;
      }

      return true;
    },
    onProfileClick(profile: Profile) {
      // If the clicked profile is not the one currently displayed
      if (profile.Id != this.currentProfile?.Id) {
        if (!this.askConfirmationBeforeSwitchingProfile()) return;

        this.currentProfile = JSON.parse(JSON.stringify(profile));

        this.resetForm();
      }
    },
    onFormConfigurationTypeChanged(event: Event) {
      if (!this.currentProfile) return;

      // Get input value
      const selectedType = +(event.target as HTMLInputElement).value;

      // For each selected screen
      for (const screen of this.selectedScreens) {
        // Get the configuration of the screen (can be null)
        const associatedConfiguration = this.getScreenConfiguration(
          screen,
          this.currentProfile.Configurations,
        );

        // If the configuration type of the screen is different than the one selected
        if (associatedConfiguration?.Discriminator != selectedType) {
          // Remove the existing configuration in the profile
          this.currentProfile.Configurations =
            this.currentProfile.Configurations.filter(
              (x) => x.DevicePath != screen.DevicePath,
            );

          // If we select to remove the configuration
          if (selectedType == -1) continue;

          let newConfiguration: Configuration;

          if (
            selectedType == ConfigurationDiscriminator.TemperatureConfiguration
          ) {
            newConfiguration = new TemperatureConfiguration({
              DevicePath: screen.DevicePath,
            });
          } else if (
            selectedType == ConfigurationDiscriminator.ColorConfiguration
          ) {
            newConfiguration = new ColorConfiguration({
              DevicePath: screen.DevicePath,
            });
          } else {
            throw new Error("Missing implementation");
          }

          // If a brightness was defined, we keep it
          if (associatedConfiguration?.Brightness != null)
            newConfiguration.Brightness = associatedConfiguration.Brightness;

          this.currentProfile.Configurations.push(newConfiguration);
        }
      }

      this.resetForm();
    },
    onFormTemperatureIntensityChanged(event: Event) {
      if (!this.currentProfile) return;

      // Get input value
      const intensity = +(event.target as HTMLInputElement).value;

      // For each selected screen
      for (const screen of this.selectedScreens) {
        // Get the configuration of the screen
        const associatedConfiguration = this.getScreenConfiguration(
          screen,
          this.currentProfile.Configurations,
        ) as TemperatureConfiguration;

        associatedConfiguration.Intensity = intensity;
      }

      this.form.temperatureIntensity = intensity;
    },
    onFormColorChanged(event: Event) {
      if (!this.currentProfile) return;

      // Get input value
      const color = (event.target as HTMLInputElement).value;

      // For each selected screen
      for (const screen of this.selectedScreens) {
        // Get the configuration of the screen
        const associatedConfiguration = this.getScreenConfiguration(
          screen,
          this.currentProfile.Configurations,
        ) as ColorConfiguration;

        associatedConfiguration.Color = color;
      }

      this.form.color = color;
    },
    onFormBrightnessChanged(event: Event) {
      if (!this.currentProfile) return;

      // Get input value
      const brightness = +(event.target as HTMLInputElement).value;

      // For each selected screen
      for (const screen of this.selectedScreens) {
        // Get the configuration of the screen
        const associatedConfiguration = this.getScreenConfiguration(
          screen,
          this.currentProfile.Configurations,
        ) as Configuration;

        associatedConfiguration.Brightness = brightness;
      }

      this.form.brightness = brightness;
    },
    async onProfileLabelClick() {
      if (!this.currentProfile) return;

      this.isEditingProfileLabel = true;
      this.profileLabelBeforeEdit = this.currentProfile.Label;

      await this.$nextTick();

      (this.$refs.profileLabelInput as HTMLInputElement).focus();
    },
    onProfileLabelInputBlur() {
      if (!this.currentProfile) return;

      this.isEditingProfileLabel = false;

      if (this.currentProfile.Label.trim() == "") {
        this.currentProfile.Label = this.profileLabelBeforeEdit;
      }
    },
    onProfileLabelInputKeyPress(event: KeyboardEvent) {
      if (event.key == "Enter") {
        (this.$refs.profileLabelInput as HTMLInputElement).blur();
      }
    },
    onNewProfileClick() {
      if (!this.askConfirmationBeforeSwitchingProfile()) return;

      const newProfile = new Profile({
        Label: "New profile",
      });

      this.profiles.push(newProfile);
      this.currentProfile = JSON.parse(JSON.stringify(newProfile));

      this.resetForm();
    },
    async onDeleteProfileClick() {
      if (!this.currentProfile) return;
      if (this.isLoading) return;

      this.isLoading = true;

      const confirmation = confirm(
        "Are you sure you want to delete this profile ?",
      );

      if (confirmation) {
        if (this.currentProfile.Id != "") {
          const result = await profileService.DeleteProfileAsync(
            this.currentProfile.Id,
          );

          if (result) {
            // Delete the profile from the list
            this.profiles.splice(
              this.profiles.findIndex((x) => x.Id == this.currentProfile?.Id),
              1,
            );
          }
        } else {
          // Delete the profile from the list
          this.profiles.splice(
            this.profiles.findIndex((x) => x.Id == this.currentProfile?.Id),
            1,
          );
        }

        this.currentProfile = undefined;
        this.resetForm();
      }

      this.isLoading = false;
    },
    onDuplicateProfileClick() {
      if (this.isButtonDuplicateDisabled) return;

      const duplicatedProfile = JSON.parse(
        JSON.stringify(this.currentProfile),
      ) as Profile;

      duplicatedProfile.Id = "";
      duplicatedProfile.Label = "Duplicated profile";
      duplicatedProfile.Configurations.forEach((x) => {
        x.Id = "";
      });

      this.profiles.push(duplicatedProfile);
      this.currentProfile = duplicatedProfile;
    },
    async onSaveProfileClick() {
      if (this.isButtonSaveDisabled) return;
      if (this.isLoading) return;

      this.isLoading = true;

      const result = await profileService.SaveProfileAsync(
        this.currentProfile!,
      );

      if (result) {
        // Delete the profile from the list
        this.profiles.splice(
          this.profiles.findIndex((x) => x.Id == this.currentProfile?.Id),
          1,
        );

        this.profiles.push(result);
      }

      this.currentProfile = result;
      this.resetForm();

      this.isLoading = false;
    },
  },
});
</script>

<template>
  <div class="h-full flex flex-col relative">
    <div
      style="
        max-height: 40%;
        min-height: 200px;
        background-color: #171717;
        padding: 50px;
      "
    >
      <ScreensViewer
        :screens="screens"
        @selectionChanged="onScreenSelectionChanged"
      ></ScreensViewer>
    </div>
    <div class="flex-1 flex overflow-hidden">
      <div
        class="flex flex-col"
        style="width: 170px; background-color: #1e1e1e"
      >
        <h1
          style="
            font-size: 1.4rem;
            text-align: center;
            margin-top: 5px;
            margin-bottom: 5px;
          "
        >
          Profiles
        </h1>
        <div
          class="overflow-y-auto overflow-x-hidden"
          style="padding-left: 5px; padding-right: 5px"
        >
          <!-- #region Profiles -->

          <TransitionGroup name="list">
            <div
              class="profile"
              v-for="profile in profiles"
              :key="profile.Id"
              @click="onProfileClick(profile)"
              @keypress.enter="onProfileClick(profile)"
              tabindex="0"
              :class="{
                selected: profile.Id == currentProfile?.Id,
                'font-bold':
                  profile.Id == currentProfile?.Id
                    ? isProfileNewOrHasChanged(currentProfile, profiles)
                    : false,
              }"
            >
              {{
                profile.Label +
                (profile.Id == currentProfile?.Id &&
                isProfileNewOrHasChanged(currentProfile, profiles)
                  ? "*"
                  : "")
              }}
            </div>
          </TransitionGroup>

          <!-- #endregion -->
        </div>
        <div class="flex-1 py-3">
          <button
            class="button block ml-auto mr-auto"
            @click="onNewProfileClick"
          >
            <font-awesome-icon icon="fa-solid fa-plus" /> New profile
          </button>
        </div>
      </div>
      <div
        class="flex-1 flex flex-col px-12 mx-auto"
        style="max-width: 750px"
        v-if="currentProfile"
      >
        <h1
          style="
            font-size: 1.4rem;
            text-align: center;
            margin-top: 5px;
            margin-bottom: 5px;
          "
        >
          Profile :
          <span
            v-show="!isEditingProfileLabel"
            tabindex="0"
            class="inline-flex items-center cursor-pointer"
            @keypress.enter="onProfileLabelClick"
            @click="onProfileLabelClick"
            >{{ currentProfile.Label }}
            <font-awesome-icon
              style="font-size: 0.5em"
              class="ml-1"
              icon="fa-solid fa-pen"
          /></span>
          <input
            v-show="isEditingProfileLabel"
            style="color: black; text-align: center"
            type="text"
            v-model="currentProfile.Label"
            @blur="onProfileLabelInputBlur"
            @keypress="onProfileLabelInputKeyPress"
            ref="profileLabelInput"
          />
        </h1>

        <div class="category">
          <h2 class="header">Color</h2>
          <div class="row">
            <div class="label">Type</div>
            <select
              :value="form.configurationType"
              @change="onFormConfigurationTypeChanged($event)"
            >
              <option value="-1"></option>
              <option
                :value="ConfigurationDiscriminator.TemperatureConfiguration"
              >
                Temperature
              </option>
              <option :value="ConfigurationDiscriminator.ColorConfiguration">
                Color
              </option>
            </select>
          </div>

          <!-- #region Type Temperature -->
          <div
            class="row"
            v-if="
              form.configurationType ==
              ConfigurationDiscriminator.TemperatureConfiguration
            "
          >
            <div class="label">Intensity</div>
            <p class="mr-1">
              {{
                form.temperatureIntensity
                  ? `${form.temperatureIntensity} K`
                  : ""
              }}
            </p>
            <input
              type="range"
              min="2000"
              max="6600"
              :value="form.temperatureIntensity"
              @change="onFormTemperatureIntensityChanged($event)"
            />
          </div>
          <!-- #endregion -->

          <!-- #region Type Color -->
          <div
            class="row"
            v-if="
              form.configurationType ==
              ConfigurationDiscriminator.ColorConfiguration
            "
          >
            <div class="label">Color</div>
            <input
              type="color"
              :value="form.color"
              @change="onFormColorChanged($event)"
            />
          </div>
          <!-- #endregion -->
        </div>

        <div class="category mt-3" v-if="showBrightness">
          <h2 class="header">Brightness</h2>
          <div class="row">
            <div class="label">Intensity</div>
            <p class="mr-1">{{ form.brightness }}</p>
            <input
              type="range"
              min="1"
              max="100"
              :value="form.brightness"
              @change="onFormBrightnessChanged($event)"
            />
            <!-- v-model="configurationBeingEdited?.Brightness" -->
          </div>
        </div>

        <div class="mt-auto mb-3 flex">
          <button
            class="button ml-auto"
            :disabled="isButtonSaveDisabled"
            @click="onSaveProfileClick()"
          >
            <font-awesome-icon icon="fa-solid fa-save" />
            Save
          </button>
          <button
            class="button ml-3"
            :disabled="isButtonDuplicateDisabled"
            @click="onDuplicateProfileClick()"
          >
            <font-awesome-icon icon="fa-solid fa-copy" /> Duplicate
          </button>
          <button
            class="button ml-3"
            style="background-color: #b40000"
            @click="onDeleteProfileClick()"
          >
            <font-awesome-icon icon="fa-solid fa-trash" /> Delete
          </button>
        </div>
      </div>
    </div>
    <div
      class="absolute inset-0"
      v-if="isLoading"
      style="background-color: black; filter: opacity(0.5)"
    ></div>
  </div>
</template>

<style scoped>
.list-enter-active,
.list-leave-active {
  transition: all 0.5s ease;
}
.list-enter-from,
.list-leave-to {
  opacity: 0;
  transform: translateX(30px);
}

.profile {
  border-radius: 5px;
  height: 30px;
  line-height: 30px;
  padding-left: 15px;
  text-overflow: ellipsis;
  white-space: nowrap;
  overflow: hidden;
}

.profile:hover {
  background-color: #2d2d2d;
}

.profile:not(:first-child):not(:last-child) {
  margin-top: 3px;
  margin-bottom: 3px;
}

.profile.selected {
  background-color: #2d2d2d;
  border-left: 3px solid #0078d7;
}

.category .header {
  font-weight: bold;
  margin-bottom: 4px;
}

.category .row {
  background-color: #2b2b2b;
  display: flex;
  min-height: 50px;
  padding: 10px 15px;
  align-items: center;
  border-left: 1px solid #1d1d1d;
  border-right: 1px solid #1d1d1d;
  border-bottom: 1px solid #1d1d1d;
}

.category .row:hover {
  background-color: #323232;
}

.category .row:first-of-type {
  border-top: 1px solid #1d1d1d;
  border-top-left-radius: 5px;
  border-top-right-radius: 5px;
}

.category .row:last-of-type {
  border-bottom-left-radius: 5px;
  border-bottom-right-radius: 5px;
}

.category .row .label {
  flex: 1;
}
</style>
