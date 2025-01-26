<script setup lang="ts">
import { deleteKeyBinding, getKeyBindings, isNullOrWhitespace, Routes, saveKeyBinding } from '@/global';
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query';
import { useToast } from 'primevue/usetoast';
import Button from 'primevue/button';
import Checkbox from 'primevue/checkbox';
import ColorPicker from 'primevue/colorpicker';
import InputText from 'primevue/inputtext';
import SelectButton from 'primevue/selectbutton';
import { computed, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { v4 as uuidv4 } from 'uuid';
import DeletePopover from '../DeletePopover.vue';
import { useSignalR } from '@/composables/useSignalR';
import { KeyBindingDto } from '@/dtos/keyBindingDto';

const props = defineProps({
  id: {
    type: String,
    required: false,
    default: undefined
  },
});

const router = useRouter();
const toast = useToast();

interface Form {
  name?: string,
}

const form = ref<Form>({
  name: "",
});

const initialForm = ref<Form>({ ...form.value });

const shouldLoadKeyBindings = computed(() => props.id != undefined);

const { data: bindings, isFetched } = useQuery({
  queryKey: ['keyBindings'],
  queryFn: getKeyBindings,
  staleTime: Infinity,
  refetchOnMount: false,
  enabled: shouldLoadKeyBindings
});

const binding = computed(() => bindings.value?.find(x => x.id == props.id));

watch([isFetched, props], () => {
  if(props.id != undefined && isFetched.value == true && binding.value == undefined)// if this binding doesn't exists
    router.push({ name: Routes.KEY_BINDINGS });
}, { immediate: true });

const reinitializeForm = () => {
  form.value.name = binding.value?.name ?? "";

  initialForm.value = { ...form.value };
};

watch([binding], () => {
  reinitializeForm();
}, { immediate: true });

const bindingFromForm = computed<KeyBindingDto>(() => {
  const dto : KeyBindingDto = {};

  return dto;
});

const { mutate: save, isSuccess: succeededSave, isError: failedSave, isPending: isSaving } = useMutation({
  mutationFn: saveKeyBinding,
});

const queryClient = useQueryClient();

watch(succeededSave, () => {
  if(succeededSave.value == true)
  {
    toast.add({ severity: "success", summary: "Success", detail: "Configuration saved.", life: 3000 });
    queryClient.invalidateQueries({ queryKey: ["keyBindings"]});
    router.push({ name: Routes.KEY_BINDINGS });
  }
});

watch(failedSave, () => {
  if(failedSave.value == true)
  {
    toast.add({ severity: "error", summary: "Failed", detail: "Failed to save binding.", life: 3000 });
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

  save(bindingFromForm.value);
};

const { mutate: deleteBinding, isSuccess: succeededDelete, isError: failedDelete, isPending: isDeleting } = useMutation({
  mutationFn: deleteKeyBinding,
});

watch(succeededDelete, () => {
  if(succeededDelete.value == true)
  {
    toast.add({ severity: "success", summary: "Success", detail: "Binding deleted.", life: 3000 });
    queryClient.invalidateQueries({ queryKey: ["keyBindings"]});
    router.push({ name: Routes.KEY_BINDINGS });
  }
});

watch(failedDelete, () => {
  if(failedDelete.value == true)
  {
    toast.add({ severity: "error", summary: "Failed", detail: "Failed to delete binding.", life: 3000 });
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
  deleteBinding(bindingFromForm.value);
};

</script>

<template>
  <div class="flex flex-col gap-2 h-full w-full">
    <Button
      class="w-fit"
      severity="secondary"
      icon="pi pi-chevron-circle-left"
      label="Back"
      @click="router.push({ name: Routes.KEY_BINDINGS })"
    />
    
    <div class="field">
      <label for="name">Name</label>
      <InputText
        id="name"
        v-model="form.name"
        placeholder="Ex: Apply 'Dimmed'"
      />
    </div>

    <!-- <div class="field">
      <p>Configuration type</p>
      <SelectButton
        v-model="form.type"
        :options="typeOptions"
        optionLabel="label"
        optionValue="value"
      />
    </div> -->

    
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
        v-if="binding != undefined"
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
    </div>
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