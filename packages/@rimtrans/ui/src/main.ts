/**
 * Main
 */
// tslint:disable:no-import-side-effect
import './main.scss';
import Vue from 'vue';
import VoidUI from 'void-ui';

Vue.use(VoidUI);

import VApp from './views/app';
import router from './router';

new Vue({
  router,
  render: h => h(VApp),
}).$mount('#app');
