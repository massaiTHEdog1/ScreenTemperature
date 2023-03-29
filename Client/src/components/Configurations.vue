<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { ScreenService } from "../services/screenService";
import ScreensViewer from "./ScreensViewer.vue";
import { Screen } from "../models/screen";

const screens = ref<Screen[]>([]);

const screenService = new ScreenService();

onMounted(() => {
  screens.value = screenService.GetScreens().map(x => new Screen(x));
});

</script>

<template>
  <div class="h-full flex flex-col">
    <div style="max-height: 40%; min-height: 200px; background-color: red; padding: 50px;">
      <ScreensViewer :screens="screens"></ScreensViewer>
    </div>
    <div class="flex-1" style="background-color: green;"></div>
  </div>
</template>

<style scoped>

.screen
{
  background-color: yellow;
  position: absolute;
}

</style>
