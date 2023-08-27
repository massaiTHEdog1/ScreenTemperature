import { createApp } from "vue";
import App from "./App.vue";
import "./style.css";

//#region FontAwesome

import { library } from "@fortawesome/fontawesome-svg-core";
import {
  faCopy,
  faDisplay,
  faGear,
  faKeyboard,
  faPen,
  faPlus,
  faSave,
  faTrash,
} from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";

library.add(
  faKeyboard,
  faDisplay,
  faGear,
  faSave,
  faTrash,
  faPen,
  faCopy,
  faPlus,
);

//#endregion

//#region Router

import router from "./router";

//#endregion

createApp(App)
  .use(router)
  .component("font-awesome-icon", FontAwesomeIcon)
  .mount("#app");
