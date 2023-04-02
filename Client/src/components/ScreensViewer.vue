<script lang="ts">
import { defineComponent, PropType, ref } from 'vue'
import { Screen } from "../models/screen";

export default defineComponent({
  expose: ["Refresh"],
  props: {
    /** A list of screen to display. */
    screens: { type: Array as PropType<Screen[]>, required: true },
    /** Enable multi-selection. */
    allowMultipleSelection: { type: Boolean, required: false, default: true },
    /** If true, a click acts as a toggle. */
    clickToggle: { type: Boolean, required: false, default: false },
    /** Allow an empty selection. */
    allowEmptySelection: { type: Boolean, required: false, default: false }
  },
  emits: ["selectionChanged"],
  data() {
    return {
      aspectRatio: ref<string>(),
    }
  },
  mounted() {
    this.Refresh();

    this.SelectFirstScreen();
  },
  watch: {
    screens(newvalue, oldValue) {
      this.Refresh();

      this.SelectFirstScreen();
    }
  },
  methods: {
    SelectFirstScreen() {
      if (!this.allowEmptySelection && this.screens.length > 0)
        this.Select(this.screens[0]);
    },
    /** Refreshes the component. */
    Refresh() {

      this.aspectRatio = undefined;

      if (this.screens.length == 0)
        return;

      let rectangleLeftBorderPosition = 0;
      let rectangleRightBorderPosition = 0;
      let rectangleTopBorderPosition = 0;
      let rectangleBottomBorderPosition = 0;

      /** The X position of the righmost screen */
      let maximumX = 0;
      /** The Y position of the lowest screen */
      let maximumY = 0;

      // We calculate the dimensions of the rectangle
      for (const screen of this.screens) {
        if (screen.X < rectangleLeftBorderPosition)
          rectangleLeftBorderPosition = screen.X;

        if (screen.X + screen.Width > rectangleRightBorderPosition)
          rectangleRightBorderPosition = screen.X + screen.Width;

        if (screen.Y < rectangleTopBorderPosition)
          rectangleTopBorderPosition = screen.Y;

        if (screen.Y + screen.Height > rectangleBottomBorderPosition)
          rectangleBottomBorderPosition = screen.Y + screen.Height;

        if (screen.X > maximumX)
          maximumX = screen.X;

        if (screen.Y > maximumY)
          maximumY = screen.Y;
      }

      // We calculate the width/height of the rectangle
      const rectangleWidth = rectangleRightBorderPosition - rectangleLeftBorderPosition;
      const rectangleHeight = rectangleBottomBorderPosition - rectangleTopBorderPosition;

      // For each screen, we calculate it's dimensions in percentage relative to the rectangle
      for (const screen of this.screens) {
        screen.WidthPercentage = screen.Width * 100 / rectangleWidth;
        screen.HeightPercentage = screen.Height * 100 / rectangleHeight;
        screen.XPercentage = (screen.X - rectangleLeftBorderPosition) * (maximumX / rectangleWidth * 100) / maximumX;
        screen.YPercentage = (screen.Y - rectangleTopBorderPosition) * (maximumY / rectangleHeight * 100) / maximumY;
      }

      this.aspectRatio = `${rectangleWidth} / ${rectangleHeight}`;
    },
    /** 
     * Deselect a screen.
     * 
     * Cannot deselect a screen if it is the only selected screen.
     * @param screen The screen we want to deselect.
     */
    Deselect(screen: Screen) {
      // A screen cannot be deselected if it is the only selected screen
      if (!this.allowEmptySelection && this.screens.filter(x => x.IsSelected)?.length == 1 && screen.DevicePath == this.screens.find(x => x.IsSelected)!.DevicePath)
        return;

      screen.IsSelected = false;
    },
    /** Select a screen. */
    Select(screen: Screen) {

      screen.IsSelected = true;

      if (!this.allowMultipleSelection)
        this.DeselectAllScreensExcept(screen);

    },
    /** Toggle selection of a screen. */
    ToggleSelection(screen: Screen) {
      if (screen.IsSelected)
        this.Deselect(screen);
      else
        this.Select(screen);
    },
    DeselectAllScreensExcept(screenToKeepSelected: Screen) {
      this.screens.forEach(screen => {
        if (screen.DevicePath != screenToKeepSelected.DevicePath)
          this.Deselect(screen);
      });
    },
    GetSimplifiedArray() {
      return this.screens.map(x => ({ id: x.DevicePath, isSelected: x.IsSelected }));
    },
    onScreenClick(event: MouseEvent, screen: Screen) {

      const before = JSON.stringify(this.GetSimplifiedArray());

      if (!this.clickToggle) {
        if (!event.ctrlKey) {
          this.Select(screen);
          // Deselect all screens except one.
          this.DeselectAllScreensExcept(screen);
        }
        else// If Ctrl key is pressed
        {
          this.ToggleSelection(screen);
        }
      }
      else {
        this.ToggleSelection(screen);
      }

      if (before != JSON.stringify(this.GetSimplifiedArray()))
        this.$emit("selectionChanged", this.screens.filter(x => x.IsSelected));
    }
  }
});

</script>

<template>
  <div style="position: relative; margin: auto; max-width: 100%; max-height: 100%;" :style="{ aspectRatio }">
    <div v-if="screens.length > 0" class="screen" v-for="screen in screens" :key="screen.DevicePath"
      @click="$event => onScreenClick($event, screen)"
      :style="{ width: `${screen.WidthPercentage}%`, height: `${screen.HeightPercentage}%`, left: `${screen.XPercentage}%`, top: `${screen.YPercentage}%` }"
      :class="{ 'selected': screen.IsSelected }">
      {{ screen.Label }}<br />
      {{ screen.Width }} x {{ screen.Height }}
    </div>
    <p v-else style="text-align: center;">No screen</p>
  </div>
</template>

<style scoped>
.screen {
  background-color: #2E2E2E;
  color: white;
  position: absolute;
  border-radius: 5px;
  border: 1px solid grey;
  user-select: none;
  -moz-user-select: none;
  -webkit-user-select: none;
  display: flex;
  justify-content: center;
  align-items: center;
  text-align: center;
}

.screen:hover {
  background-color: #454545;
}

.screen.selected {
  background-color: #0078D7;
}
</style>
