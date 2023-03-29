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
      screensContainer: ref<HTMLElement>()
    }
  },
  mounted() {
    this.GenerateView();
  },
  watch: {
    screens(newvalue, oldValue)
    {
      this.GenerateView();
    }
  },
  methods: {
    GenerateView() {
      if (!this.screens)
        return;

      let rectangleMinimumX = 0;
      let rectangleMaximumX = 0;
      let rectangleMinimumY = 0;
      let rectangleMaximumY = 0;
      let maximumX = 0;
      let maximumY = 0;

      // We calculate the dimensions of the rectangle
      for (const screen of this.screens) {
        if (screen.X < rectangleMinimumX)
          rectangleMinimumX = screen.X;

        if (screen.X + screen.Width > rectangleMaximumX)
          rectangleMaximumX = screen.X + screen.Width;

        if (screen.Y < rectangleMinimumY)
          rectangleMinimumY = screen.Y;

        if (screen.Y + screen.Height > rectangleMaximumY)
          rectangleMaximumY = screen.Y + screen.Height;

        if (screen.X > maximumX)
          maximumX = screen.X;

        if (screen.Y > maximumY)
          maximumY = screen.Y;
      }

      const width = rectangleMaximumX - rectangleMinimumX;
      const height = rectangleMaximumY - rectangleMinimumY;

      // For each screen, we calculate it's dimensions in percentage relative to the rectangle
      for (const screen of this.screens) {
        screen.WidthPercentage = screen.Width * 100 / width;
        screen.HeightPercentage = screen.Height * 100 / height;
        screen.XPercentage = (screen.X - rectangleMinimumX) * (maximumX / width * 100) / maximumX;
        screen.YPercentage = (screen.Y - rectangleMinimumY) * (maximumY / height * 100) / maximumY;
      }

      this.aspectRatio = `${width} / ${height}`;

      // console.log(minimumX, minimumY, width, height);

      // (screensContainer.value as HTMLElement).innerHTML = "WAZABI";
    }
  }
});

</script>

<template>
  <div class="h-full w-full">
    <div ref="screensContainer" class="max-w-full max-h-full m-auto relative" style="background-color: blue;"
      :style="{ aspectRatio }">
      <div class="screen" v-for="screen in screens"
        :style="{ width: `${screen.WidthPercentage}%`, height: `${screen.HeightPercentage}%`, left: `${screen.XPercentage}%`, top: `${screen.YPercentage}%` }">
        {{ screen.Label }}
      </div>
    </div>
  </div>
</template>

<style scoped>
.screen {
  background-color: yellow;
  position: absolute;
}
</style>
