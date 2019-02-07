import Vue from 'vue';
import VueRouter from 'vue-router';

Vue.use(VueRouter);

import { VHome } from './views/home';
import { VConfigs } from './views/configs/index';

/**
 * Router
 */
export const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  routes: [
    {
      path: '/',
      name: 'home',
      component: VHome,
    },
    {
      path: '/configs',
      component: VConfigs,
    },
  ],
});
