<script setup lang="ts">
import { onMounted, ref } from 'vue'

onMounted(() => {
  console.log("Bindings");
});

// defineProps<{ msg: string }>()

const count = ref(0)
</script>

<template>
  <div>
    <button type="button" @click="count++">count is {{ count }}</button>
  </div>
</template>

<style scoped>

</style>
