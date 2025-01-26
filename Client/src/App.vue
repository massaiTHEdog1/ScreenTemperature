<script setup lang="ts">
import ScreensViewer from '@/components/ScreensViewer.vue';
import { useRoute } from 'vue-router';
import ProgressSpinner from 'primevue/progressspinner';
import { useScreens } from '@/composables/useScreens';
import Toast from 'primevue/toast';
import { ref } from 'vue';
import { useSignalR } from './composables/useSignalR';

const route = useRoute();

const { isFetching: isFetchingScreens, isError: failedFetchingScreens } = useScreens();

const transitionTime = ref(150);

useSignalR();

</script>

<template>
  <div class="h-full flex flex-col gap-2 bg-[#1B2126] text-white p-3">
    <div class="w-full h-fit max-h-[min(50%,250px)] p-5 bg-[#171717] mx-auto rounded-lg border-2 border-[#4D4D4D]">
      <ScreensViewer>
        <template #no-screen>
          <div class="flex items-center">
            <ProgressSpinner v-if="isFetchingScreens || failedFetchingScreens" />
          </div>
        </template>
      </ScreensViewer>
    </div>
    
    <div
      class="wrapper overflow-x-hidden h-full w-full relative"
      style="container-type: inline-size;"
    >
      <div
        class="big-block h-full flex flex-row transition-[left] absolute"
        :style="{ 
          left: `${(route.matched.length - 1) * -100}%`, 
          width: `${(route.matched.length + 1) * 100}%`,
          transitionDuration: `${transitionTime}ms`
        }"
      >
        <transition-group :duration="transitionTime">
          <div 
            class="slide h-full w-[100cqw]"
            v-for="matched in route.matched"
            :key="matched.name"
          >
            <div class=" w-full h-full mx-auto max-w-[850px]">
              <component
                :is="matched.components?.default"
                v-bind="typeof matched.props.default === 'function' ? matched.props.default(route) : undefined"
              />
            </div>
          </div>
        </transition-group>
      </div>
    </div>
  </div>
  <Toast />
</template>

<style scoped>


</style>
