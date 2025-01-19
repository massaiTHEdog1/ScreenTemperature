import { RouteRecordRaw, createRouter, createWebHistory } from "vue-router";
import { Routes } from "@/global";
import CategorySelectionPage from "./components/Pages/CategorySelectionPage.vue";
import ConfigurationsPage from "./components/Pages/ConfigurationsPage.vue";
import CreateOrUpdateConfigurationPage from "./components/Pages/CreateOrUpdateConfigurationPage.vue";

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
    ]
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes, // short for `routes: routes`
});

export default router;
