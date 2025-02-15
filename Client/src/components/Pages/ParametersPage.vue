<script setup lang="ts">
import { ParametersDto } from '@/dtos/parameters';
import { Routes, getParameters, updateParameters } from '@/global';
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query';
import { Checkbox, useToast } from 'primevue';
import Button from 'primevue/button';
import ProgressSpinner from 'primevue/progressspinner';
import { computed, ref, watch } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();

const { data: parameters, isFetching: isFetchingParameters, isError: failedFetchingParameters } = useQuery({
  queryKey: ['parameters'],
  queryFn: getParameters,
  staleTime: Infinity
});

interface Form {
  startApplicationOnUserLogin?: boolean,
  minimizeOnStartup?: boolean,
}

const initialForm = computed<Form>(() => (
  {
    startApplicationOnUserLogin: parameters.value?.startApplicationOnUserLogin ?? false,
    minimizeOnStartup: parameters.value?.minimizeOnStartup ?? false,
  }
));

const form = ref<Form>({...initialForm.value});

watch(initialForm, () => {
  form.value = {...initialForm.value};
}, { immediate: true });

const parametersFromForm = computed<ParametersDto>(() => (
  {
    startApplicationOnUserLogin: form.value.startApplicationOnUserLogin ?? false,
    minimizeOnStartup: form.value.minimizeOnStartup ?? false
  }
));

const shouldSave = ref(false);

watch(parametersFromForm, () => {
  if(shouldSave.value == false) return;

  shouldSave.value = false;
  mutate(parametersFromForm.value);
});



const { mutate, isSuccess: succeededSave, isError: failedSave, isPending: isSaving, data: saveDataResponse } = useMutation({
  mutationFn: updateParameters,
});

const queryClient = useQueryClient();
const toast = useToast();

watch(succeededSave, () => {
  if(succeededSave.value == true)
  {
    // toast.add({ severity: "success", summary: "Success", detail: "Parameters saved.", life: 3000 });
    queryClient.setQueryData(["parameters"], saveDataResponse.value);
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
          inputId="startOnLogin"
          :disabled="isSaving"
          :modelValue="form.startApplicationOnUserLogin"
          @update:model-value="(e) => {
            shouldSave = true;
            form.startApplicationOnUserLogin = e;
          }"
        />
        <label for="startOnLogin">Start application on user login</label>
      </div>

      <div class="flex items-center gap-2">
        <Checkbox
          binary  
          inputId="minimizeOnStartup"
          :disabled="isSaving"
          :modelValue="form.minimizeOnStartup"
          @update:model-value="(e) => {
            shouldSave = true;
            form.minimizeOnStartup = e;
          }"
        />
        <label for="minimizeOnStartup">Start application minimized in system tray</label>
      </div>
    </div>
  </div>
</template>

<style scoped>
</style>