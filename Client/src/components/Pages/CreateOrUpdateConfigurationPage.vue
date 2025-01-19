
<script setup lang="ts">
import { useScreens } from '@/composables/useScreens';
import { ColorConfigurationDto } from '@/dtos/configurations/colorConfigurationDto';
import { isColorConfigurationApplyResult, isTemperatureConfigurationApplyResult } from '@/dtos/configurations/configurationApplyResultDto';
import { ConfigurationDiscriminator, ConfigurationDto } from '@/dtos/configurations/configurationDto';
import { TemperatureConfigurationDto } from '@/dtos/configurations/temperatureConfigurationDto';
import { Routes, applyConfiguration, getConfigurations, isNullOrWhitespace, saveConfiguration } from '@/global';
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query';
import { useToast } from 'primevue/usetoast';
import Button from 'primevue/button';
import Checkbox from 'primevue/checkbox';
import ColorPicker from 'primevue/colorpicker';
import InputText from 'primevue/inputtext';
import SelectButton from 'primevue/selectbutton';
import Slider from 'primevue/slider';
import { computed, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { v4 as uuidv4 } from 'uuid';

const props = defineProps({
  id: {
    type: String,
    required: false,
    default: undefined
  },
});

interface Form {
  type?: ConfigurationDiscriminator,
  name?: string,
  isBrightnessChecked?: boolean,
  brightness?: number,
  isTemperatureChecked?: boolean,
  temperature?: number,
  isColorChecked?: boolean,
  color?: string,
}

const form = ref<Form>({
  type: ConfigurationDiscriminator.TemperatureConfiguration,
  name: "",
  isBrightnessChecked: false,
  brightness: 100,
  isTemperatureChecked: true,
  temperature: 6600,
  isColorChecked: false,
  color: "FFFFFF",
});

const initialForm = ref<Form>({ ...form.value });

const shouldLoadConfigurations = computed(() => props.id != undefined);

const { data: configurations } = useQuery({
  queryKey: ['configurations'],
  queryFn: getConfigurations,
  staleTime: Infinity,
  refetchOnMount: false,
  enabled: shouldLoadConfigurations
});

const { selectedScreens, screens } = useScreens();
const selectedScreen = computed(() => selectedScreens.value?.[0]);

const isBrightnessSupported = computed(() => selectedScreen.value?.isBrightnessSupported == true);

const configuration = computed(() => configurations.value?.find(x => x.id == props.id));

const reinitializeForm = () => {
  form.value.type = configuration.value?.$type ?? ConfigurationDiscriminator.TemperatureConfiguration;
  form.value.name = configuration.value?.name ?? "";
  form.value.isBrightnessChecked = configuration.value?.applyBrightness ?? selectedScreen.value?.isBrightnessSupported ?? false;
  form.value.brightness = configuration.value?.brightness ?? 100;
  form.value.isTemperatureChecked = (configuration.value as TemperatureConfigurationDto)?.applyIntensity ?? true;
  form.value.temperature = (configuration.value as TemperatureConfigurationDto)?.intensity ?? 6600;
  form.value.isColorChecked = (configuration.value as ColorConfigurationDto)?.applyColor ?? false;
  form.value.color = (configuration.value as ColorConfigurationDto)?.color ?? "FFFFFF";

  initialForm.value = { ...form.value };
};

watch([configuration], () => {
  reinitializeForm();
}, { immediate: true });

const isFirstScreenLoading = ref(true);

watch(screens, () => {
  if(isFirstScreenLoading.value == true)
  {
    const screenToSelect = screens.value?.find(x => x.id == configuration.value?.devicePath);

    if(screenToSelect != undefined)
    {
      selectedScreens.value = [screenToSelect];
      isFirstScreenLoading.value = false;

      reinitializeForm();
    }
  }
}, { immediate: true });

const typeOptions : { label: string, value: ConfigurationDiscriminator }[] = [
  { 
    label: "Temperature", 
    value: ConfigurationDiscriminator.TemperatureConfiguration 
  }, 
  {
    label: "Color", 
    value: ConfigurationDiscriminator.ColorConfiguration
  }
];

const configurationFromForm = computed<ConfigurationDto>(() => {
  let dto : TemperatureConfigurationDto | ColorConfigurationDto;

  if(form.value.type == ConfigurationDiscriminator.TemperatureConfiguration) 
  {
    dto = {
      $type: ConfigurationDiscriminator.TemperatureConfiguration,
      id: configuration.value?.id ?? uuidv4(),
      devicePath: selectedScreen.value?.id ?? "",
      name: form.value.name ?? "",
      applyBrightness: form.value.isBrightnessChecked ?? false,
      brightness: form.value.brightness ?? 0,
      applyIntensity: form.value.isTemperatureChecked ?? false,
      intensity: form.value.temperature ?? 0,
    } satisfies TemperatureConfigurationDto;
  }
  else if(form.value.type == ConfigurationDiscriminator.ColorConfiguration) 
  {
    dto = {
      $type: ConfigurationDiscriminator.ColorConfiguration,
      id: configuration.value?.id ?? uuidv4(),
      devicePath: selectedScreen.value?.id ?? "",
      name: form.value.name ?? "",
      applyBrightness: form.value.isBrightnessChecked ?? false,
      brightness: form.value.brightness ?? 0,
      applyColor: form.value.isColorChecked ?? false,
      color: `#${form.value.color ?? "FFFFFF"}`,
    } satisfies ColorConfigurationDto;
  }
  else
  {
    throw Error("Not implemented");
  }

  return dto;
});

const router = useRouter();

const { mutate: apply, isSuccess: succeededApply, isError: failedApply, data: applyResult, isPending: isApplying } = useMutation({
  mutationFn: applyConfiguration,
});

const toast = useToast();

watch(failedApply, () => {
  if(failedApply.value == true)
  {
    toast.add({ severity: "error", summary: "Error", detail: "An error occured.", life: 3000 });
  }
});

watch(succeededApply, () => {
  if(succeededApply.value == true && applyResult.value != undefined)
  {
    if(form.value.isBrightnessChecked == true){
      if(applyResult.value.succeededToApplyBrightness == false)
        toast.add({ severity: "error", summary: "Error", detail: "Failed to apply brightness.", life: 3000 });
      else
        toast.add({ severity: "success", summary: "Success", detail: "Brightness applied.", life: 3000 });
    }

    if(isTemperatureConfigurationApplyResult(applyResult.value)){
      if(applyResult.value.succeededToApplyTemperature == false)
        toast.add({ severity: "error", summary: "Error", detail: "Failed to apply temperature.", life: 3000 });
      else
        toast.add({ severity: "success", summary: "Success", detail: "Temperature applied.", life: 3000 });
    }

    if(isColorConfigurationApplyResult(applyResult.value)) {
      if(applyResult.value.succeededToApplyColor == false)
        toast.add({ severity: "error", summary: "Error", detail: "Failed to apply color.", life: 3000 });
      else
        toast.add({ severity: "success", summary: "Success", detail: "Color applied.", life: 3000 });
    }

    queryClient.invalidateQueries({ queryKey: ["screens"] });
  }
});

const isButtonApplyDisabled = computed(() => isApplying.value);

const onApplyClick = () => {

  if(isButtonApplyDisabled.value == true) return;

  apply(configurationFromForm.value);
};

const { mutate: save, isSuccess: succeededSave, isError: failedSave, isPending: isSaving } = useMutation({
  mutationFn: saveConfiguration,
});

const queryClient = useQueryClient();

watch(succeededSave, () => {
  if(succeededSave.value == true)
  {
    toast.add({ severity: "success", summary: "Success", detail: "Configuration saved.", life: 3000 });
    queryClient.invalidateQueries({ queryKey: ["configurations"]});
    router.push({ name: Routes.CONFIGURATIONS });
  }
});

watch(failedSave, () => {
  if(failedSave.value == true)
  {
    toast.add({ severity: "error", summary: "Failed", detail: "Failed to save configuration.", life: 3000 });
  }
});

const isButtonSaveDisabled = computed(() => 
  isSaving.value == true || 
  isNullOrWhitespace(form.value.name) == true || 
  JSON.stringify(form.value) == JSON.stringify(initialForm.value) // if form is unchanged
);

const onSaveClick = () => {
  if(isButtonSaveDisabled.value) return;  

  save(configurationFromForm.value);
};

</script>

<template>
  <div class="flex flex-col gap-2 h-full w-full">
    <Button
      class="w-fit"
      severity="secondary"
      icon="pi pi-chevron-circle-left"
      label="Back"
      @click="router.push({ name: Routes.CONFIGURATIONS })"
    />

    <div
      v-if="(selectedScreens?.length ?? 0) != 1"
      class="text-center"
    >
      Please select a screen.
    </div>
    <template v-else>
      <div class="field">
        <p>Target screen</p>
        <p>{{ `${selectedScreens![0].index} - ${selectedScreens![0].label}` }}</p>
      </div>
      
      <div class="field">
        <label for="name">Name</label>
        <InputText
          id="name"
          v-model="form.name"
          placeholder="Ex: Dimmed"
        />
      </div>

      <div class="field">
        <p>Configuration type</p>
        <SelectButton
          v-model="form.type"
          :options="typeOptions"
          optionLabel="label"
          optionValue="value"
        />
      </div>

      <template v-if="form.type == ConfigurationDiscriminator.TemperatureConfiguration">
        <div class="field">
          <div class="flex gap-2 items-center">
            <Checkbox
              v-model="form.isTemperatureChecked"
              :binary="true"
              input-id="temperature"
            />
            <label for="temperature">Temperature</label>
          </div>
          <div class="flex gap-2 items-center">
            <Slider
              v-model="form.temperature"
              :min="2000"
              :max="6600"
              :disabled="!form.isTemperatureChecked"
              class="flex-1 m-3 temperature-slider"
            />
            <p class="w-[60px] text-right">
              {{ form.temperature }} K
            </p>
          </div>
        </div>
      </template>
      <template v-else-if="form.type == ConfigurationDiscriminator.ColorConfiguration">
        <div class="field">
          <div class="flex gap-2 items-center">
            <Checkbox
              v-model="form.isColorChecked"
              :binary="true"
              input-id="color"
            />
            <label for="color">Color</label>
          </div>
          <ColorPicker
            v-model="form.color"
            :disabled="!form.isColorChecked"
          />
        </div>
      </template>

      <div class="field">
        <div class="flex gap-2 items-center">
          <Checkbox
            :disabled="!isBrightnessSupported"
            v-model="form.isBrightnessChecked"
            :binary="true"
            input-id="brightness"
          />
          <label for="brightness">Brightness</label>
        </div>
        <div class="flex gap-2 items-center">
          <Slider
            v-model="form.brightness"
            :disabled="!isBrightnessSupported || !form.isBrightnessChecked"
            class="flex-1 m-3"
          />
          <p class="w-[25px] text-right">
            {{ form.brightness }}
          </p>
        </div>
      </div>

      <div class="flex gap-2">
        <Button
          class="w-fit"
          severity="primary"
          icon="pi pi-save"
          label="Save"
          @click="onSaveClick"
          :loading="isSaving"
          :disabled="isButtonSaveDisabled"
        />

        <Button
          class="w-fit"
          severity="secondary"
          label="Apply"
          @click="onApplyClick"
          :loading="isApplying"
          :disabled="isButtonApplyDisabled"
        />
      </div>
    </template>
  </div>
</template>

<style scoped>
:deep(.p-selectbutton .p-button.p-highlight::before)
{
  background-color: var(--primary-color);
}

:deep(.p-selectbutton .p-button.p-highlight)
{
  color: white;
}

.field {
  @apply bg-slate-700 flex flex-col gap-3 p-3 rounded-lg
}

:deep(.temperature-slider .p-slider-range) {
  @apply bg-transparent;
}

:deep(.temperature-slider)
{
  @apply bg-gradient-to-r from-orange-400 to-white;
}
</style>