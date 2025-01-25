<script setup lang="ts">
import { useScreens } from "@/composables/useScreens"; 

const { selectedScreens, screens, aspectRatio } = useScreens();

</script>

<template>
  <div
    class="relative mx-auto max-h-full"
    :style="{ aspectRatio }"
  >
    <div
      v-for="screen in screens"
      v-if="screens.length > 0"
      
      :key="screen.id"
      class="screen"
      :style="{ width: `${screen.widthPercentage}%`, height: `${screen.heightPercentage}%`, left: `${screen.XPercentage}%`, top: `${screen.YPercentage}%` }"
      :class="{ 'selected': selectedScreens.some(x => x == screen.id) }"
      tabindex="0"
      @keyup.enter="() => selectedScreens = [screen.id]"
      @click="() => selectedScreens = [screen.id]"
    >
      <p class="absolute text-sm top-1 left-1">
        {{ screen.index }}
      </p>
      <p>{{ screen.label }}</p>
      <p>{{ screen.width }} x {{ screen.height }}</p>
      <p
        v-tooltip.top="'Current brightness'"
        v-if="screen.isBrightnessSupported"
      >
        <i class="pi pi-lightbulb" /> {{ (screen.currentBrightness! - screen.brightnessMinimum!) * 100 / (screen.brightnessMaximum! - screen.brightnessMinimum!) }}%
      </p>
    </div>
    <slot
      v-else
      name="no-screen"
    >
      <slot name="no-screen">
        <p class="text-center">
          No screen
        </p>
      </slot>
    </slot>
  </div>
</template>

<style scoped>
.screen {
  @apply truncate flex-col relative; 
  background-color: #2E2E2E;
  color: white;
  position: absolute;
  border-radius: 5px;
  display: flex;
  justify-content: center;
  align-items: center;
  text-align: center;
  border: 1px solid transparent;
  background-clip: padding-box;
}

.screen:hover {
  background-color: #454545;
  border: 2px solid white !important;
}

.screen.selected {
  background-color: #0078D7;
  border: 1px solid white;
}
</style>
