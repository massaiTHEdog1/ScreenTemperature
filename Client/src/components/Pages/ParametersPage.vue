<script setup lang="ts">
import { Routes, getParameters, updateParameters } from '@/global';
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query';
import { Checkbox, useToast } from 'primevue';
import Button from 'primevue/button';
import ProgressSpinner from 'primevue/progressspinner';
import { watch } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();

const { data: parameters, isFetching: isFetchingParameters, isError: failedFetchingParameters } = useQuery({
  queryKey: ['parameters'],
  queryFn: getParameters,
  staleTime: Infinity
});

const updateStartParameter = (newvalue: boolean) => {
  mutate({
    ...parameters.value,
    startApplicationOnUserLogin: newvalue
  });
};

const { mutate, isSuccess: succeededSave, isError: failedSave, isPending: isSaving } = useMutation({
  mutationFn: updateParameters,
});

const queryClient = useQueryClient();
const toast = useToast();

watch(succeededSave, () => {
  if(succeededSave.value == true)
  {
    toast.add({ severity: "success", summary: "Success", detail: "Parameters saved.", life: 3000 });
    queryClient.invalidateQueries({ queryKey: ["parameters"]});
  }
});

watch(failedSave, () => {
  if(failedSave.value == true)
  {
    toast.add({ severity: "error", summary: "Failed", detail: "Failed to save parameters.", life: 3000 });
  }
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

    <ProgressSpinner v-if="isFetchingParameters || failedFetchingParameters" />
    <div
      v-else
      class="flex-1 flex flex-col mt-5 gap-8 items-center"
    >
      <div class="flex items-center gap-2">
        <Checkbox
          binary  
          inputId="startup"
          :disabled="isSaving"
          :modelValue="parameters?.startApplicationOnUserLogin"
          @update:model-value="(e) => updateStartParameter(e)"
        />
        <label for="startup">Start application on user login</label>
      </div>
    </div>
  </div>
</template>

<style scoped>
</style>