<script setup lang="tsx">
import { Button } from 'primevue';
import Popover, { PopoverMethods } from 'primevue/popover';
import { ref } from 'vue';

const popover = ref<PopoverMethods>();

const show = (e: MouseEvent) => {
  popover.value?.show(e);
};

const hide = () => {
  popover.value?.hide();
};

defineExpose({
  show, 
  hide
});

defineEmits<{
  onDeleteClick: [],
  onCancelClick: []
}>();

</script>
<template>
  <Popover ref="popover">
    <div class="flex flex-col gap-2">
      <p>Are you sure you want to delete ?</p>
      <div class="flex gap-2 justify-end">
        <Button
          severity="danger"
          icon="pi pi-trash"
          label="Delete"
          @click="$emit('onDeleteClick')"
        />
        
        <Button
          severity="secondary"
          icon="pi pi-times"
          label="Cancel"
          @click="$emit('onCancelClick')"
        />
      </div>
    </div>
  </Popover>
</template>