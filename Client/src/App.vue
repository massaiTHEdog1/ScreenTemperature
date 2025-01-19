<script setup lang="ts">
import ScreensViewer from '@/components/ScreensViewer.vue';
import { Routes } from "@/global";
import { computed } from 'vue';
import { useRoute } from 'vue-router';
import ProgressSpinner from 'primevue/progressspinner';
import { useScreens } from '@/composables/useScreens';
import Toast from 'primevue/toast';

const route = useRoute();

const { selectedScreens, screens, isFetching: isFetchingScreens, isError: failedFetchingScreens } = useScreens();

const currentPageLevel = computed(() => {
  if(route.name == Routes.CONFIGURATIONS || route.name == Routes.BINDINGS) return 1;
  if(route.name == Routes.CONFIGURATIONS_CREATE || route.name == Routes.CONFIGURATIONS_UPDATE) return 2;
  return 0;
});

const marginLeft = computed(() => {
  return `-${currentPageLevel.value * 100}%`;
});

</script>

<template>
  <div class="h-full flex flex-col gap-2 bg-[#1B2126] text-white p-3">
    <div class="w-full h-fit max-h-[min(50%,250px)] p-5 bg-[#171717] mx-auto rounded-lg border-2 border-[#4D4D4D]">
      <ScreensViewer
        :screens="screens ?? []"
        :allow-empty-selection="true"
        :allow-multiple-selection="false"
        @selection-changed="(e) => selectedScreens = e"
      >
        <template #no-screen>
          <div class="flex items-center">
            <ProgressSpinner v-if="isFetchingScreens || failedFetchingScreens" />
          </div>
        </template>
      </ScreensViewer>
    </div>
    
    <div class="overflow-x-hidden h-full w-full">
      <div
        class="h-full w-[300%] flex flex-row transition-all"
        :style="{ marginLeft: marginLeft }"
      >
        <div class="h-full w-full">
          <div class="content w-full h-full mx-auto">
            <component
              :is="route.matched.find(x => x.name == Routes.CATEGORY_SELECTION)?.components?.default"
              v-if="route.matched.some(x => x.name == Routes.CATEGORY_SELECTION)"
            />
          </div>
        </div>
        <div class="h-full w-full">
          <div class="content w-full h-full mx-auto">
            <component
              :is="route.matched.find(x => x.name == Routes.CONFIGURATIONS)?.components?.default"
              v-if="route.matched.some(x => x.name == Routes.CONFIGURATIONS)"
            />
          </div>
        </div>
        <div class="h-full w-full">
          <div class="content w-full h-full mx-auto">
            <component
              :is="route.matched.find(x => x.name == Routes.CONFIGURATIONS_CREATE)?.components?.default"
              v-if="route.matched.some(x => x.name == Routes.CONFIGURATIONS_CREATE)"
            />
            <component
              :is="route.matched.find(x => x.name == Routes.CONFIGURATIONS_UPDATE)?.components?.default"
              v-bind="route.matched.find(x => x.name == Routes.CONFIGURATIONS_UPDATE)?.props.default(route)"
              v-if="route.matched.some(x => x.name == Routes.CONFIGURATIONS_UPDATE)"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
  <Toast/>
</template>

<style scoped>

.content {
  max-width: 850px;
}

</style>
