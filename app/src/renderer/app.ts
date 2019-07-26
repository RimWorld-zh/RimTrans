/* eslint-disable @typescript-eslint/no-explicit-any */
import Vue, { PluginObject } from 'vue';
import VueRouter from 'vue-router';
import { PluginIpc, States, I18n, StatesUI } from './utils';
import * as Components from './components';
import { createRouter } from './router';
import VApp from './views/app';

Vue.use(PluginIpc);
Vue.use(States);
Vue.use(I18n);
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
  const i18n = new I18n({ states });
  const ui = new StatesUI({ states });

  const router = createRouter();

  const app = new Vue({
    i18n,
    states,
    router,
    render: h => h(VApp),
  });

  return { app, router };
}
