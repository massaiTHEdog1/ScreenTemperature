<script lang="ts">
import { defineComponent, PropType, ref } from 'vue'
import { ScreenService } from "../services/screenService";
import ScreensViewer from "./ScreensViewer.vue";
import { Screen } from "../models/screen";
import { ProfileService } from '../services/profileService';
import { Profile } from '../models/profile';
import { Configuration, ConfigurationDiscriminator } from '../models/configurations/configuration';
import { ColorConfiguration } from '../models/configurations/colorConfiguration';
import { TemperatureConfiguration } from '../models/configurations/temperatureConfiguration';

export default defineComponent({
  components: {
    ScreensViewer
  },
  data() {
    return {
      screenService: new ScreenService(),
      configurationService: new ProfileService(),
      screens: ref<Screen[]>([]),
      profiles: ref<Profile[]>([]),

      /** The displayed {@link Profile} */
      profileCopy: ref<Profile>(new Profile()),
      /** The displayed {@link Configuration} */
      configurationCopy: ref<Configuration>(),
      /** The displayed brightness */
      brightness: ref<number>(100),
      isEditingProfileLabel: ref<boolean>(false),

      profileLabelBeforeEdit: "",

      ConfigurationDiscriminator: ConfigurationDiscriminator
    }
  },
  mounted() {
    // Load the screens and the profiles
    this.screens = this.screenService.GetScreens().map(x => new Screen(x));
    this.profiles = this.configurationService.GetConfigurations();

    // If there is at least one saved profile
    if (this.profiles.length > 0) {
      // Show the first profile
      this.profileCopy = JSON.parse(JSON.stringify(this.profiles[0]));
    }
  },
  computed: {
    selectedScreens() {
      return this.screens.filter(x => x.IsSelected);
    }
  },
  methods: {
    /** Returns true if the profile has changed. */
    hasProfileChanged(profile: Profile) {
      return profile.Id == 0 || (profile.Id == this.profileCopy.Id && JSON.stringify(profile) != JSON.stringify(this.profileCopy));
    },
    onScreenSelectionChanged(selectedScreens: Screen[]) {

      const values = [];

      // For each selected screen
      for (const screen of this.selectedScreens) {
        // Get the associated configuration
        let configuration = this.profileCopy.Configurations.find(x => x.DevicePath == screen.DevicePath);

        // If there is an associated configuration
        if (configuration) {
          // Copy the configuration
          configuration = JSON.parse(JSON.stringify(configuration)) as Configuration;

          // Reset fields so we can compare them
          configuration.Id = 0;
          configuration.DevicePath = "";
        }

        // Add the value to the list
        values.push(configuration);
      }

      let allEquals = true;

      // Store the first value for comparison
      const firstValueAsString = JSON.stringify(values[0]);

      // For each selected screen except the first one
      for (let i = 1; i < values.length; i++) {
        // If the associated configuration is different than the one of the first selected screen
        if (JSON.stringify(values[i]) != firstValueAsString) {
          allEquals = false;
          break;
        }
      }

      // If the configuration of all the selected screens are the same
      if (allEquals && firstValueAsString)
        this.configurationCopy = JSON.parse(firstValueAsString);
      else
        this.configurationCopy = undefined;
    },
    /** Display a modal asking for confirmation before switching profile. */
    askConfirmationBeforeSwitchingProfile(): boolean{

      const profilCopyInList = this.profiles.find(x => x.Id == this.profileCopy.Id);
      // If the current profile is a new one or it has not been saved
      if (this.profileCopy.Id == 0 || this.hasProfileChanged(profilCopyInList!)) {
        const confirmation = confirm("You have not saved this profile. You will lose all your changes. \nAre you sure ?");

        // If this profile is a new one
        if(this.profileCopy.Id == 0 && confirmation)
        {
          // Delete the profile from the list
          this.profiles.splice(this.profiles.length-1, 1);

        }

        return confirmation;
      }

      return true;
    },
    onProfileClick(profile: Profile) {

      // If the clicked profile is not the one currently displayed
      if (profile.Id != this.profileCopy.Id) {

        if(!this.askConfirmationBeforeSwitchingProfile())
          return;

        this.profileCopy = JSON.parse(JSON.stringify(profile));

        // Display the configuration of the selected screen
        if (this.selectedScreens.length > 1)
          this.configurationCopy = undefined;
        else
          this.configurationCopy = JSON.parse(JSON.stringify(profile.Configurations.find(x => x.DevicePath == this.selectedScreens[0].DevicePath)));
      }
    },
    // Triggered when the value of the combobox is changed
    onConfigurationTypeSelectionChanged(event: Event) {

      const value = +(event.target as HTMLInputElement).value;

      if (value == ConfigurationDiscriminator.ColorConfiguration) {
        this.configurationCopy = new ColorConfiguration();
      }
      else if (value == ConfigurationDiscriminator.TemperatureConfiguration) {
        this.configurationCopy = new TemperatureConfiguration();
      }
      else {
        throw new Error("Missing implementation");
      }

      this.applyConfigurationToSelectedScreens();
    },
    applyConfigurationToSelectedScreens() {

      if (this.profileCopy) {
        // Remove all the previous configurations for the current profile
        this.profileCopy.Configurations = this.profileCopy.Configurations.filter(x => !this.selectedScreens.map(x => x.DevicePath).includes(x.DevicePath));
      }

      // for each selected screen
      for (const selectedScreen of this.selectedScreens) {
        const copy = JSON.parse(JSON.stringify(this.configurationCopy)) as Configuration;
        copy.DevicePath = selectedScreen.DevicePath;

        // we add this configuration to the current profile
        this.profileCopy.Configurations.push(copy);
      }
    },
    async onProfileLabelClick() {
      this.isEditingProfileLabel = true;
      this.profileLabelBeforeEdit = this.profileCopy.Label;

      await this.$nextTick();

      (this.$refs.profileLabelInput as HTMLInputElement).focus();
    },
    onProfileLabelInputBlur() {
      this.isEditingProfileLabel = false;

      if (this.profileCopy.Label.trim() == "") {
        this.profileCopy.Label = this.profileLabelBeforeEdit;
      }
    },
    onProfileLabelInputKeyPress(event: KeyboardEvent) {
      if (event.key == "Enter") {
        (this.$refs.profileLabelInput as HTMLInputElement).blur();
      }
    },
    onNewProfileClick() {

      if(!this.askConfirmationBeforeSwitchingProfile())
          return;

      const newProfile = new Profile({
        Label: "New profile"
      });

      this.profiles.push(newProfile);
      this.profileCopy = JSON.parse(JSON.stringify(newProfile));

      this.configurationCopy = new TemperatureConfiguration();
    }
  }
});

</script>

<template>
  <div class="h-full flex flex-col">
    <div style="max-height: 40%; min-height: 200px; background-color: #171717; padding: 50px;">
      <ScreensViewer :screens="screens" @selectionChanged="onScreenSelectionChanged"></ScreensViewer>
    </div>
    <div class="flex-1 flex overflow-hidden">
      <div class="flex flex-col" style="width: 170px; background-color: #1E1E1E;">
        <h1 style="font-size: 1.4rem; text-align: center; margin-top: 5px; margin-bottom: 5px;">Profiles</h1>
        <div class="overflow-y-auto overflow-x-hidden" style="padding-left: 5px; padding-right: 5px;">

          <!-- #region Profiles -->

          <TransitionGroup name="list">
            <div class="profile flex items-center" v-for="profile in profiles" :key="profile.Id"
              @click="onProfileClick(profile)" @keypress.enter="onProfileClick(profile)" tabindex="0"
              :class="{ 'selected': profile.Id == profileCopy.Id, 'font-bold': hasProfileChanged(profile) }">
              {{ profile.Label + (hasProfileChanged(profile) ? '*' : '') }}
            </div>
          </TransitionGroup>

          <!-- #endregion -->

        </div>
        <div class="flex-1 py-3">
          <button class="button block ml-auto mr-auto" style="background-color: #0078D7;" @click="onNewProfileClick">
            <font-awesome-icon icon="fa-solid fa-plus" /> New profile
          </button>
        </div>
      </div>
      <div class="flex-1 flex flex-col px-12 mx-auto" style="max-width: 750px;">
        <h1 style="font-size: 1.4rem; text-align: center; margin-top: 5px; margin-bottom: 5px;">
          Profile :
          <span v-show="!isEditingProfileLabel" tabindex="0" class="inline-flex items-center cursor-pointer"
            @keypress.enter="onProfileLabelClick" @click="onProfileLabelClick">{{ profileCopy.Label }}
            <font-awesome-icon style="font-size: 0.5em;" class="ml-1" icon="fa-solid fa-pen" /></span>
          <input v-show="isEditingProfileLabel" style="color: black; text-align: center;" type="text"
            v-model="profileCopy.Label" @blur="onProfileLabelInputBlur" @keypress="onProfileLabelInputKeyPress"
            ref="profileLabelInput" />
        </h1>

        <div class="category">

          <h2 class="header">Color</h2>
          <div class="row">
            <div class="label">Type</div>
            <select :value="configurationCopy?.Discriminator" @change="onConfigurationTypeSelectionChanged">
              <option :value="ConfigurationDiscriminator.TemperatureConfiguration">Temperature</option>
              <option :value="ConfigurationDiscriminator.ColorConfiguration">Color</option>
            </select>
          </div>

          <!-- #region Type Temperature -->
          <div class="row" v-if="configurationCopy?.Discriminator == ConfigurationDiscriminator.TemperatureConfiguration">
            <div class="label">Intensity</div>
            <p class="mr-1">{{ (configurationCopy as TemperatureConfiguration).Intensity }} K</p>
            <input type="range" min="2000" max="6600" v-model="(configurationCopy as TemperatureConfiguration).Intensity"
              @change="applyConfigurationToSelectedScreens()" />
          </div>
          <!-- #endregion -->

          <!-- #region Type Color -->
          <div class="row" v-if="configurationCopy?.Discriminator == ConfigurationDiscriminator.ColorConfiguration">
            <div class="label">Color</div>
            <input type="color" v-model="(configurationCopy as ColorConfiguration).Color"
              @change="applyConfigurationToSelectedScreens()" />
          </div>
          <!-- #endregion -->

        </div>

        <div class="category mt-3">

          <h2 class="header">Brightness</h2>
          <div class="row">
            <div class="label">Intensity</div>
            <p class="mr-1">{{ brightness }}</p>
            <input type="range" min="1" max="100" v-model="brightness" />
          </div>

        </div>

        <div class="mt-auto mb-3 flex">
          <button class="button ml-auto" style="background-color: #0078D7;">
            <font-awesome-icon icon="fa-solid fa-save" /> Save
          </button>
          <button class="button ml-3" style="background-color: #0078D7;">
            <font-awesome-icon icon="fa-solid fa-copy" /> Duplicate
          </button>
          <button class="button ml-3" style="background-color: red;">
            <font-awesome-icon icon="fa-solid fa-trash" /> Delete
          </button>
        </div>
      </div>

    </div>
  </div>
</template>

<style scoped>

.list-enter-active, .list-leave-active {
  transition: all 0.5s ease;
}
.list-enter-from, .list-leave-to {
  opacity: 0;
  transform: translateX(30px);
}
.button {
  height: 30px;
  line-height: 30px;
  padding-left: 10px;
  padding-right: 10px;
  cursor: pointer;
}

.profile {
  border-radius: 5px;
  height: 30px;
  padding-left: 15px;
}

.profile:hover {
  background-color: #2D2D2D;
}

.profile:not(:first-child):not(:last-child) {
  margin-top: 3px;
  margin-bottom: 3px;
}

.profile.selected {
  background-color: #2D2D2D;
  border-left: 3px solid #0078D7;
}

.category .header {
  font-weight: bold;
  margin-bottom: 4px;
}

.category .row {
  background-color: #2B2B2B;
  display: flex;
  min-height: 50px;
  padding: 10px 15px;
  align-items: center;
  border-left: 1px solid #1D1D1D;
  border-right: 1px solid #1D1D1D;
  border-bottom: 1px solid #1D1D1D;
}

.category .row:hover {
  background-color: #323232;
}

.category .row:first-of-type {
  border-top: 1px solid #1D1D1D;
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
