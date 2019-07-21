import Vue from 'vue';
import VueRouter from 'vue-router';
import { States } from './utils';
import * as Components from './components/all';
import VApp from './views/app';
import { createRouter } from './router';

Vue.use(VueRouter);
Vue.use(States);
Object.entries(Components).forEach(([name, comp]) => Vue.component(name, comp));

Vue.config.productionTip = false;

export function createApp(): { app: Vue; router: VueRouter } {
  const states = new States();
  const router = createRouter();
  const app = new Vue({
    states,
    router,
    render: h => h(VApp),
  });

  return { app, router };
}
