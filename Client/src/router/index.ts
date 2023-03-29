import { createWebHashHistory, createRouter, RouteRecordRaw } from "vue-router";
import KeyBindings from "../components/KeyBindings.vue";
import Configurations from "../components/Configurations.vue";
import Settings from "../components/Settings.vue";
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