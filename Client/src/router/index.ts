import { createWebHashHistory, createRouter, RouteRecordRaw } from "vue-router";
import KeyBindings from "../components/KeyBindingsPage.vue";
import Configurations from "../components/ConfigurationsPage.vue";
import Settings from "../components/SettingsPage.vue";
import ROUTES from "./constants";

const routes : RouteRecordRaw[] = [
  { path: ROUTES.CONFIGURATIONS, component: Configurations },
  { path: ROUTES.BINDINGS, component: KeyBindings },
  { path: ROUTES.SETTINGS, component: Settings },
];

const router = createRouter({
  history: createWebHashHistory(),
  routes, // short for `routes: routes`
});

export default router;