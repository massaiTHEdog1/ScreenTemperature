import { createApp } from "vue";
import App from "./App.vue";
import "./style.css";
import 'primeicons/primeicons.css';
import PrimeVue from 'primevue/config';
import { VueQueryPlugin } from '@tanstack/vue-query';
import router from "@/router";
import "@fortawesome/fontawesome-free/css/all.min.css";
import Tooltip from 'primevue/tooltip';
import Aura from '@primevue/themes/aura';
import ToastService from 'primevue/toastservice';

createApp(App)
  .directive('tooltip', Tooltip)
  .use(router)
  .use(PrimeVue, {
    theme: {
      preset: Aura,
      options: {
        darkModeSelector: '.my-app-dark'
      }
    }
  })
  .use(VueQueryPlugin)
  .use(ToastService)
  .mount("#app");
