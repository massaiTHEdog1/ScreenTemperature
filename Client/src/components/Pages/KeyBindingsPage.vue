<script setup lang="ts">
import { Routes, getKeyBindings } from '@/global';
import { useQuery } from '@tanstack/vue-query';
import Button from 'primevue/button';
import { useRouter } from 'vue-router';
import SquareButton from "@/components/SquareButton.vue";
import ProgressSpinner from 'primevue/progressspinner';

const router = useRouter();

const { data: bindings, isFetching: isFetchingBindings, isError: failedFetchingBindings } = useQuery({
  queryKey: ['keyBindings'],
  queryFn: getKeyBindings,
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
    <ProgressSpinner v-if="isFetchingBindings || failedFetchingBindings" />
    <div
      v-else
      class="flex-1 flex flex-col mt-5 gap-8 items-center"
    >
      <p v-if="(bindings?.length ?? 0) == 0">
        You don't have any binding... Yet !
      </p>
      <div
        v-else
        class="flex gap-2"
      >
        <SquareButton 
          v-for="element in bindings" 
          :key="element.id"
          :label="element.name"
          class="!w-[100px] !h-[100px]"
          @click="router.push({ name: Routes.KEY_BINDING_UPDATE, params: { id: element.id} })"
          @keyup.enter="router.push({ name: Routes.KEY_BINDING_UPDATE, params: { id: element.id} })"
        />
      </div>
      <SquareButton
        label="Add"
        icon="pi pi-plus-circle text-2xl"
        class="max-w-[100px] max-h-[100px]"
        @click="router.push({ name: Routes.KEY_BINDING_CREATE })"
        @keyup.enter="router.push({ name: Routes.KEY_BINDING_CREATE })"
      />
    </div>
  </div>
</template>

<style scoped>
</style>