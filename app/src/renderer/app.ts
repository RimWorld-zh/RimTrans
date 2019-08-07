/* eslint-disable @typescript-eslint/no-explicit-any */
import Vue, { PluginObject } from 'vue';
import VueRouter from 'vue-router';
import { States } from './utils';
import * as Components from './components';
import { createRouter } from './router';
import VApp from './views/app';

Vue.use(States);
Object.entries(Components).forEach(([name, comp]) => {
  Vue.component(name, comp);
  if ('install' in comp && typeof (comp as PluginObject<any>).install === 'function') {
    Vue.use(comp);
  }
});
Vue.use(VueRouter);

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
