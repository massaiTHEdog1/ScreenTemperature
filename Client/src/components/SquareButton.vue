<script setup lang="ts">

defineProps({
  icon: {
    type: String,
    required: false,
    default: undefined
  },
  label: {
    type: String,
    required: false,
    default: undefined
  }
});

</script>

<template>
  <div
    class="w-full h-full bg-slate-700 rounded-lg cursor-pointer hover:border flex flex-col gap-2 items-center justify-center text-center"
    tabindex="0"
  >
    <i
      v-if="icon != undefined"
      :class="icon"
    />
    <p v-if="label != undefined">
      {{ label }}
    </p>
  </div>
</template>

<style scoped>

</style>