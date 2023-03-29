<script lang="ts">
import { defineComponent, PropType, ref } from 'vue'
import { Screen } from "../models/screen";

export default defineComponent({
  props: {
    screens: { type: Object as PropType<Screen[]>, required: true },
  },
  data() {
    return {
      aspectRatio: ref("1/1"),
    }
  },
  mounted() {
    this.GenerateView();
  },
  watch: {
    screens(newvalue, oldValue) {
      this.GenerateView();
    }
  },
  methods: {
    GenerateView() {
      if (!this.screens)
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
    onScreenClick(event: MouseEvent, screen: Screen) {

      if(!event.ctrlKey)
      {
        this.screens.forEach(x => x.IsSelected = false);
      }

      screen.IsSelected = !screen.IsSelected;
    }
  }
});

</script>

<template>
  <div class="h-full w-full">
    <div class="max-w-full max-h-full m-auto relative" style="background-color: blue;" :style="{ aspectRatio }">
      <div class="screen flex items-center justify-center" v-for="screen in screens" @click="$event => onScreenClick($event, screen)"
        :style="{ width: `${screen.WidthPercentage}%`, height: `${screen.HeightPercentage}%`, left: `${screen.XPercentage}%`, top: `${screen.YPercentage}%` }"
        :class="{ 'selected': screen.IsSelected }">
        {{ screen.Label }}
      </div>
    </div>
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
}

.screen:hover {
  background-color: #454545;
}

.screen.selected {
  background-color: #0078D7;
}
</style>
