import { createApp } from 'vue';
import './style.css';
import App from './App.vue';

//#region FontAwesome

import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import { library } from "@fortawesome/fontawesome-svg-core";
import { faDisplay, faGear, faKeyboard } from "@fortawesome/free-solid-svg-icons";

library.add(faKeyboard, faDisplay, faGear);

//#endregion

//#region Router

import router from './router';

//#endregion

createApp(App)
    .use(router)
    .component('font-awesome-icon', FontAwesomeIcon)
    .mount('#app');
