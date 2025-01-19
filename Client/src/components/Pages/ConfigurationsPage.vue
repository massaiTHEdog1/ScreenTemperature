<script setup lang="ts">
import { Routes, getConfigurations } from '@/global';
import { useQuery } from '@tanstack/vue-query';
import Button from 'primevue/button';
import { useRouter } from 'vue-router';
import SquareButton from "@/components/SquareButton.vue";
import ProgressSpinner from 'primevue/progressspinner';

const router = useRouter();

const { data: configurations, isFetching: isFetchingConfigurations, isError: failedFetchingConfigurations } = useQuery({
  queryKey: ['configurations'],
  queryFn: getConfigurations,
  staleTime: Infinity
});

</script>

<template>
  <div class="flex flex-col gap-2 h-full w-full">
    <Button
      class="w-fit"
      severity="secondary"
      icon="pi pi-chevron-circle-left"
      label="Back"
      @click="router.push({ name: Routes.CATEGORY_SELECTION })"
    />
    <ProgressSpinner v-if="isFetchingConfigurations || failedFetchingConfigurations" />
    <div
      v-else
      class="flex-1 flex flex-col mt-5 gap-8 items-center"
    >
      <p v-if="(configurations?.length ?? 0) == 0">
        You don't have any configuration... Yet !
      </p>
      <div v-else class="flex gap-2">
        <SquareButton 
          v-for="element in configurations" 
          :key="element.id"
          :label="element.name"
          class="w-[100px] h-[100px]"
          @click="router.push({ name: Routes.CONFIGURATIONS_UPDATE, params: { id: element.id} })"
          @keyup.enter="router.push({ name: Routes.CONFIGURATIONS_UPDATE, params: { id: element.id} })"
        />
      </div>
      <SquareButton
        label="Add"
        icon="pi pi-plus-circle text-2xl"
        class="max-w-[100px] max-h-[100px]"
        @click="router.push({ name: Routes.CONFIGURATIONS_CREATE })"
        @keyup.enter="router.push({ name: Routes.CONFIGURATIONS_CREATE })"
      />
    </div>
  </div>
</template>

<style scoped>
</style>