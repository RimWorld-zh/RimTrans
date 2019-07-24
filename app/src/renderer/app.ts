import Vue from 'vue';
import VueRouter from 'vue-router';
import { I18n, States } from './utils';
import * as Components from './components';
import { createRouter } from './router';
import VApp from './views/app';

Vue.use(I18n);
Vue.use(States);
Object.entries(Components).forEach(([name, comp]) => Vue.component(name, comp));
Vue.use(VueRouter);

Vue.config.productionTip = false;

export function createApp(): { app: Vue; router: VueRouter } {
  const i18n = new I18n();
  const states = new States();
  const router = createRouter();
  const app = new Vue({
    i18n,
    states,
    router,
    render: h => h(VApp),
  });

  return { app, router };
}
