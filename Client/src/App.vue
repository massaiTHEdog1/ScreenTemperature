<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import ROUTES from './router/constants';

const router = useRouter();
const route = useRoute();

const currentRoutePath = ref(ROUTES.CONFIGURATIONS);

watch(() => route.path, path => {
  currentRoutePath.value = path;
});

</script>

<template>
  <div class="h-full flex">
    <div class="menu">
      
      <div class="menu-item" :class="{ 'menu-item-active': currentRoutePath == ROUTES.CONFIGURATIONS }" @click=" router.push(ROUTES.CONFIGURATIONS)">
        <font-awesome-icon icon="fa-solid fa-display" />
      </div>

      <div class="menu-item" :class="{ 'menu-item-active': currentRoutePath == ROUTES.BINDINGS }" @click=" router.push(ROUTES.BINDINGS)">
        <font-awesome-icon icon="fa-solid fa-keyboard" />
      </div>

      <div class="menu-item mt-auto" :class="{ 'menu-item-active': currentRoutePath == ROUTES.SETTINGS }" @click=" router.push(ROUTES.SETTINGS)">
        <font-awesome-icon icon="fa-solid fa-gear" />
      </div>

    </div>
    <div class="flex-1" style="background-color: #252526;">
      <router-view />
    </div>
  </div>
  <!-- <HelloWorld msg="Vite + Vue" /> -->
</template>

<style scoped>
* {
  --menu-width: 50px;
}

.menu {
  display: flex;
  flex-direction: column;
  height: 100%;
  width: var(--menu-width);
  background-color: #333333;
}

.menu .menu-item {
  width: var(--menu-width);
  height: var(--menu-width);

  cursor: pointer;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  font-size: 1.4rem;
  color: #858585;
}

.menu .menu-item:hover {
  color: white;
}

.menu .menu-item-active {
  color: white;
}

.menu .menu-item-active::before {
  content: '';
  /* background-color: white; */
  position: absolute;
  width: var(--menu-width);
  height: var(--menu-width);
  border-left: 3px solid white;
}

</style>
