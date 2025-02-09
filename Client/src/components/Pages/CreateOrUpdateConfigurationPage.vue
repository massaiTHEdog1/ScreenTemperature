<script setup lang="ts">
import { useScreens } from '@/composables/useScreens';
import { ColorConfigurationDto } from '@/dtos/configurations/colorConfigurationDto';
import { ConfigurationDiscriminator, ConfigurationDto } from '@/dtos/configurations/configurationDto';
import { TemperatureConfigurationDto } from '@/dtos/configurations/temperatureConfigurationDto';
import { Routes, deleteConfiguration, getConfigurations, isNullOrWhitespace, saveConfiguration } from '@/global';
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
import DeletePopover from '../DeletePopover.vue';
import { useSignalR } from '@/composables/useSignalR';

const props = defineProps<{ id?: string }>();

const router = useRouter();
const toast = useToast();
const isUpdateMode = computed(() => props.id != undefined);

const { selectedScreens, screens } = useScreens();
const selectedScreen = computed(() => screens.value.find(x => x.id == selectedScreens.value[0]));

const { data: configurations, isFetching: isFetchingConfigurations } = useQuery({
  queryKey: ['configurations'],
  queryFn: getConfigurations,
  staleTime: Infinity,
  refetchOnMount: false,
  enabled: isUpdateMode
});

const isBrightnessSupported = computed(() => selectedScreen.value?.isBrightnessSupported == true);

const configurationToUpdate = computed(() => configurations.value?.find(x => x.id == props.id));
const screenAssociatedToConfigurationToUpdate = computed(() => screens.value?.find(x => x.id == configurationToUpdate.value?.devicePath));

interface Form {
  screen?: string,
  type?: ConfigurationDiscriminator,
  name?: string,
  isBrightnessChecked?: boolean,
  brightness?: number,
  isTemperatureChecked?: boolean,
  temperature?: number,
  isColorChecked?: boolean,
  color?: string,
}

const initialForm = computed<Form>(() => ({
  screen: configurationToUpdate.value?.devicePath ?? selectedScreen.value?.id,
  type: configurationToUpdate.value?.$type ?? ConfigurationDiscriminator.TemperatureConfiguration,
  name: configurationToUpdate.value?.name ?? "",
  isBrightnessChecked: configurationToUpdate.value?.applyBrightness ?? selectedScreen.value?.isBrightnessSupported ?? false,
  brightness: configurationToUpdate.value?.brightness ?? 100,
  isTemperatureChecked: (configurationToUpdate.value as TemperatureConfigurationDto)?.applyIntensity ?? true,
  temperature: (configurationToUpdate.value as TemperatureConfigurationDto)?.intensity ?? 6600,
  isColorChecked: (configurationToUpdate.value as ColorConfigurationDto)?.applyColor ?? false,
  color: (configurationToUpdate.value as ColorConfigurationDto)?.color ?? "FFFFFF",
}));

const form = ref<Form>({...initialForm.value});

watch(initialForm, () => {
  form.value = {...initialForm.value};
}, { immediate: true });

watch([isFetchingConfigurations], () => {
  if(isUpdateMode.value == true && isFetchingConfigurations.value == false && configurationToUpdate.value == undefined)// if this configuration doesn't exists
  {
    toast.add({ severity: "error", summary: "Failed", detail: "Configuration not found.", life: 3000 });
    router.push({ name: Routes.CONFIGURATIONS });
  }
}, { immediate: true });

watch([selectedScreen], () => {
  form.value.screen = selectedScreen.value?.id;
});

watch(screenAssociatedToConfigurationToUpdate, () => {
  if(screenAssociatedToConfigurationToUpdate.value != undefined)
  {
    selectedScreens.value = [screenAssociatedToConfigurationToUpdate.value.id];
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
      id: configurationToUpdate.value?.id ?? uuidv4(),
      devicePath: form.value?.screen ?? "",
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
      id: configurationToUpdate.value?.id ?? uuidv4(),
      devicePath: form.value?.screen ?? "",
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
  isDeleting.value == true ||
  isNullOrWhitespace(form.value.name) == true || 
  JSON.stringify(form.value) == JSON.stringify(initialForm.value) // if form is unchanged
);

const onSaveClick = () => {
  if(isButtonSaveDisabled.value) return;  

  save(configurationFromForm.value);
};

const { mutate: deleteConf, isSuccess: succeededDelete, isError: failedDelete, isPending: isDeleting } = useMutation({
  mutationFn: deleteConfiguration,
});

watch(succeededDelete, () => {
  if(succeededDelete.value == true)
  {
    toast.add({ severity: "success", summary: "Success", detail: "Configuration deleted.", life: 3000 });
    queryClient.invalidateQueries({ queryKey: ["configurations"]});
    router.push({ name: Routes.CONFIGURATIONS });
  }
});

watch(failedDelete, () => {
  if(failedDelete.value == true)
  {
    toast.add({ severity: "error", summary: "Failed", detail: "Failed to delete configuration.", life: 3000 });
  }
});

const isButtonDeleteDisabled = computed(() => 
  isSaving.value == true ||
  isDeleting.value == true
);

const deletePopover = ref<InstanceType<typeof DeletePopover>>();

const onDeleteClick = (e: MouseEvent) => {
  if(isButtonDeleteDisabled.value) return;

  deletePopover.value!.show(e);
};

const onDeleteSecondClick = () => {
  deletePopover.value!.hide();
  deleteConf(configurationFromForm.value);
};

const { connection, on } = useSignalR();


const receivedLastApplyBrightnessResult = ref(true);

const applyBrightness = async (value: number) => {

  if(receivedLastApplyBrightnessResult.value == false) return;

  receivedLastApplyBrightnessResult.value = false;

  connection.value.invoke("ApplyBrightness", value, selectedScreen.value?.id);
};

on("ApplyBrightnessResult", (result: boolean) => { 

  receivedLastApplyBrightnessResult.value = true;

  if(result == false)
  {
    //console.log("ERROR");
  }
});


const receivedLastApplyTemperatureResult = ref(true);
const showInvalidTemperature = ref(false);

const applyTemperature = async (value: number) => {

  if(receivedLastApplyTemperatureResult.value == false) return;

  receivedLastApplyTemperatureResult.value = false;

  connection.value.invoke("ApplyTemperature", value, selectedScreen.value?.id);
};

on("ApplyTemperatureResult", (result: boolean) => { 

  receivedLastApplyTemperatureResult.value = true;

  if(result == false)
  {
    showInvalidTemperature.value = true;
  }
  else
  {
    showInvalidTemperature.value = false;
  }
});


const receivedLastApplyColorResult = ref(true);

const applyColor = async (value: string) => {

  if(receivedLastApplyColorResult.value == false) return;

  receivedLastApplyColorResult.value = false;

  connection.value.invoke("ApplyColor", value, selectedScreen.value?.id);
};

on("ApplyColorResult", (result: boolean) => { 

  receivedLastApplyColorResult.value = true;

  if(result == false)
  {
    //console.log("ERROR");
  }
});


const isButtonApplyDisabled = computed(() => isSaving.value || isDeleting.value);

const onApplyClick = () => {

  if(isButtonApplyDisabled.value == true) return;

  if(form.value.isBrightnessChecked)
    applyBrightness(form.value.brightness!);

  if(form.value.type == ConfigurationDiscriminator.TemperatureConfiguration && form.value.isTemperatureChecked)
    applyTemperature(form.value.temperature!);

  if(form.value.type == ConfigurationDiscriminator.ColorConfiguration && form.value.isColorChecked)
    applyColor(form.value.color!);
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
      v-if="selectedScreens.length != 1"
      class="text-center"
    >
      Please select a screen.
    </div>
    <template v-else>
      <div class="field">
        <p>Target screen</p>
        <p>{{ `${selectedScreen?.index} - ${selectedScreen?.label}` }}</p>
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
              inputId="temperature"
            />
            <label for="temperature">Temperature</label>
          </div>
          <div class="flex gap-2 items-center">
            <Slider
              v-model="form.temperature"
              @update:model-value="(e : number | number[]) => applyTemperature(e as number)"
              :min="2000"
              :max="6600"
              :disabled="!form.isTemperatureChecked"
              class="flex-1 m-3 temperature-slider"
            />
            <p class="w-[60px] text-right">
              {{ form.temperature }} K
            </p>
          </div>
          <p
            v-if="showInvalidTemperature"
            class="text-red-500"
          >
            This value is not supported.
          </p>
        </div>
      </template>
      <template v-else-if="form.type == ConfigurationDiscriminator.ColorConfiguration">
        <div class="field">
          <div class="flex gap-2 items-center">
            <Checkbox
              v-model="form.isColorChecked"
              :binary="true"
              inputId="color"
            />
            <label for="color">Color</label>
          </div>
          <ColorPicker
            v-model="form.color"
            @update:model-value="(e : string) => applyColor(`#${e ?? 'FFFFFF'}`)"
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
            inputId="brightness"
          />
          <label for="brightness">Brightness</label>
        </div>
        <div class="flex gap-2 items-center">
          <Slider
            v-model="form.brightness"
            @update:model-value="(e : number | number[]) => applyBrightness(e as number)"
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
          v-if="isUpdateMode"
          class="w-fit"
          severity="danger"
          icon="pi pi-trash"
          label="Delete"
          @click="onDeleteClick"
          :loading="isDeleting"
          :disabled="isButtonDeleteDisabled"
        />

        <DeletePopover
          ref="deletePopover"
          @on-delete-click="onDeleteSecondClick"
          @on-cancel-click="deletePopover?.hide()"
        />

        <Button
          class="w-fit"
          severity="secondary"
          label="Apply"
          @click="onApplyClick"
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