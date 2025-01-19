<script setup lang="ts">
import { Routes } from '@/global';
import { RouteLocationRaw, useRouter } from 'vue-router';
import SquareButton from '../SquareButton.vue';

const router = useRouter();

const items : { 
  label: string, 
  icon: string, 
  to: RouteLocationRaw 
}[] = [
  {
    icon: "fa-solid fa-sliders",
    label: "Configurations",
    to: { name: Routes.CONFIGURATIONS }
  },
  {
    icon: "fa-regular fa-keyboard",
    label: "Key bindings",
    to: { name: Routes.BINDINGS }
  },
  {
    icon: "fa-solid fa-gear",
    label: "Parameters",
    to: { name: Routes.PARAMETERS }
  }
];

</script>

<template>
  <div class="flex flex-col h-full items-center justify-around sm:flex-row gap-2">
    <SquareButton
      v-for="(item, index) in items"
      :key="index"
      class="max-w-[200px] max-h-[200px]"
      :tabindex="router.currentRoute.value.name == Routes.CATEGORY_SELECTION ? 0 : -1"
      :icon="`${item.icon} text-3xl`"
      :label="item.label"
      @click="router.push(item.to)"
      @keyup.enter="router.push(item.to)"
    />
  </div>
</template>

<style scoped>

</style>