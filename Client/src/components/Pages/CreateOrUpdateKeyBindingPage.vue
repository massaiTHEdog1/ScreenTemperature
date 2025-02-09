<script setup lang="ts">
import { deleteKeyBinding, getConfigurations, getKeyBindings, isNullOrWhitespace, Routes, saveKeyBinding } from '@/global';
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
import { Dialog, MultiSelect, Select } from 'primevue';

const props = defineProps<{ id?: string }>();

const router = useRouter();
const toast = useToast();



const isUpdateMode = computed(() => props.id != undefined);

const { data: bindings, isFetching: isFetchingBindings } = useQuery({
  queryKey: ['keyBindings'],
  queryFn: getKeyBindings,
  staleTime: Infinity,
  refetchOnMount: false,
  enabled: isUpdateMode
});

const bindingToUpdate = computed(() => bindings.value?.find(x => x.id == props.id));

watch([isFetchingBindings], () => {
  if(isUpdateMode.value == true && isFetchingBindings.value == false && bindingToUpdate.value == undefined)// if this binding doesn't exists
  {
    toast.add({ severity: "error", summary: "Failed", detail: "Binding not found.", life: 3000 });
    router.push({ name: Routes.KEY_BINDINGS });
  }
}, { immediate: true });

interface Form {
  name?: string,
  keys?: { 
    ctrl: boolean,
    alt: boolean,
    keyCode: number
  },
  configurationIds?: string[]
}

const initialForm = computed<Form>(() => ({
  name: bindingToUpdate.value?.name ?? "",
  keys: bindingToUpdate.value != undefined && 
    { 
      ctrl: bindingToUpdate.value.control,
      alt: bindingToUpdate.value.alt,
      keyCode: bindingToUpdate.value.keyCode
    } || undefined,
  configurationIds: bindingToUpdate.value?.configurationIds
}));

const form = ref<Form>({...initialForm.value});

watch(initialForm, () => {
  form.value = {...initialForm.value};
}, { immediate: true });

const bindingFromForm = computed<KeyBindingDto>(() => {
  const dto : KeyBindingDto = {
    id: bindingToUpdate.value?.id ?? uuidv4(),
    name: form.value.name ?? "",
    control: form.value.keys?.ctrl ?? false,
    alt: form.value.keys?.alt ?? false,
    keyCode: form.value.keys?.keyCode ?? 0,
    configurationIds: form.value.configurationIds ?? []
  };
  
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
  form.value.keys?.keyCode == undefined ||
  (form.value.configurationIds?.length ?? 0) == 0 ||
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

const showAssignKeysDialog = ref(false);

const keys = ref<{
  isCtrlKeypressed?: boolean,
  isAltKeyPressed?: boolean,
  keyPressed?: number
}>({});

const onKeyPress = (e: KeyboardEvent) => {

  e.preventDefault();
  e.stopImmediatePropagation();

  if(e.repeat) return;
  
  keys.value.isCtrlKeypressed = e.ctrlKey;
  keys.value.isAltKeyPressed = e.altKey;
  keys.value.keyPressed = e.key == "Control" || e.key == "Alt" ? undefined : e.keyCode;
};

watch(showAssignKeysDialog, () => {
  if(showAssignKeysDialog.value == true)
    document.addEventListener("keydown", onKeyPress);
  else
  {
    document.removeEventListener("keydown", onKeyPress);
    keys.value = {};
  }
});

const keysToDisplay = computed(() => {
  const keys : string[] = [];

  if(form.value.keys?.ctrl)
    keys.push("CTRL");

  if(form.value.keys?.alt)
    keys.push("ALT");

  if(form.value.keys?.keyCode)
    keys.push(form.value.keys.keyCode.toString());

  return keys.join(" + ");
});

const onAssignKeysDialogConfirm = () => {
  form.value.keys = {
    ctrl: keys.value.isCtrlKeypressed ?? false,
    alt: keys.value.isAltKeyPressed ?? false,
    keyCode: keys.value.keyPressed!,
  };

  showAssignKeysDialog.value = false;
};

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
      @click="router.push({ name: Routes.KEY_BINDINGS })"
    />
    
    <div class="field">
      <label for="name">Name</label>
      <InputText
        id="name"
        v-model="form.name"
        placeholder="Ex: Apply Dimmed"
      />
    </div>

    <div class="field">
      <label for="name">Keys</label>
      <p
        class="text-yellow-500"
        v-if="form.keys == undefined"
      >
        You did not define a combination of keys.
      </p>
      <p v-else>
        {{ keysToDisplay }}
      </p>
      <Button
        class="w-fit"
        @click="() => showAssignKeysDialog = true"
      >
        Assign keys
      </Button>
    </div>

    <div class="field">
      <label for="name">Configurations to apply</label>
      <MultiSelect
        id="name"
        v-model="form.configurationIds"
        placeholder="Select a Configuration"
        :options="configurations"
        optionLabel="name"
        optionValue="id"
        :loading="isFetchingConfigurations || failedFetchingConfigurations"
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
    </div>
  </div>

  <Dialog
    v-model:visible="showAssignKeysDialog"
    modal
    :style="{ width: '30rem', height: '15rem' }"
    :showHeader="false"
    dismissableMask
    contentClass="w-full h-full flex flex-col gap-2"
    class
  >
    <div class="flex-1 flex flex-col gap-8 items-center justify-center">
      <p>Press the combination of keys you would like to assign !</p>
      <div class="flex gap-8 items-center justify-center">
        <transition>
          <div
            class="key"
            v-if="keys.isCtrlKeypressed"
          >
            CTRL
          </div>
        </transition>
  
        <transition>
          <div
            class="key"
            v-if="keys.isAltKeyPressed"
          >
            ALT
          </div>
        </transition>
  
        <transition>
          <div
            class="key"
            v-if="keys.keyPressed != undefined"
          >
            {{ keys.keyPressed }}
          </div>
        </transition>
      </div>
    </div>
    <div class="flex justify-end gap-2">
      <Button
        class="w-fit"
        severity="primary"
        label="Confirm"
        :disabled="keys.keyPressed == undefined"
        @click="onAssignKeysDialogConfirm"
      />
      <Button
        class="w-fit"
        severity="secondary"
        label="Cancel"
        @click="showAssignKeysDialog = false"
      />
    </div>
  </Dialog>
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

.key
{
  @apply h-[70px] min-w-[70px] px-2 rounded-xl border-white border-2 border-dashed flex justify-center items-center font-bold text-xl;
}

.v-enter-active,
.v-leave-active {
  @apply transition-all duration-300;
}

.v-enter-from,
.v-leave-to {
  @apply w-0 h-0 text-[0px];
}

</style>