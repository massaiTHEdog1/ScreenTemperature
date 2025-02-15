import { Routes } from "@/global";
import { RouteRecordRaw, createRouter, createWebHistory } from "vue-router";
import CategorySelectionPage from "./components/Pages/CategorySelectionPage.vue";
import ConfigurationsPage from "./components/Pages/ConfigurationsPage.vue";
import CreateOrUpdateConfigurationPage from "./components/Pages/CreateOrUpdateConfigurationPage.vue";
import CreateOrUpdateKeyBindingPage from "./components/Pages/CreateOrUpdateKeyBindingPage.vue";
import KeyBindingsPage from "./components/Pages/KeyBindingsPage.vue";
import ParametersPage from "./components/Pages/ParametersPage.vue";

const routes: RouteRecordRaw[] = [
  { 
    name: Routes.CATEGORY_SELECTION,
    path: "/", 
    component: CategorySelectionPage,
    children: [
      { 
        name: Routes.CONFIGURATIONS,
        path: "configurations", 
        component: ConfigurationsPage,
        children: [
          {
            name: Routes.CONFIGURATIONS_CREATE,
            path: "create",
            component: CreateOrUpdateConfigurationPage
          },
          {
            name: Routes.CONFIGURATIONS_UPDATE,
            path: ":id",
            component: CreateOrUpdateConfigurationPage,
            props: route => ({ id: route.params.id })
          }
        ]
      },
      { 
        name: Routes.KEY_BINDINGS,
        path: "bindings", 
        component: KeyBindingsPage,
        children: [
          {
            name: Routes.KEY_BINDING_CREATE,
            path: "create",
            component: CreateOrUpdateKeyBindingPage
          },
          {
            name: Routes.KEY_BINDING_UPDATE,
            path: ":id",
            component: CreateOrUpdateKeyBindingPage,
            props: route => ({ id: route.params.id })
          }
        ]
      },
      { 
        name: Routes.PARAMETERS,
        path: "parameters", 
        component: ParametersPage,
      },
    ]
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes, // short for `routes: routes`
});

export default router;
