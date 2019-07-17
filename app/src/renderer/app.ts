import Vue from 'vue';
import VueRouter from 'vue-router';
import VApp from './views/app';
import { createRouter } from './router';

Vue.use(VueRouter);

Vue.config.productionTip = false;

export function createApp(): { app: Vue; router: VueRouter } {
  const router = createRouter();
  const app = new Vue({
    router,
    render: h => h(VApp),
  });

  return { app, router };
}
