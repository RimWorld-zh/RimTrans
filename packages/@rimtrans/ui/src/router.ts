import Vue from 'vue';
import VueRouter from 'vue-router';
Vue.use(VueRouter);

import { VHome } from './views/home';
import { VConfigs } from './views/configs/index';
import { VInterfaceLanguages } from './views/configs/interface-languages';
import { VCoreLanguages } from './views/configs/core-languages';

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
      children: [
        {
          path: '',
          name: 'configs',
          redirect: 'interface-languages',
        },
        {
          path: 'interface-languages',
          name: 'interface-languages',
          component: VInterfaceLanguages,
        },
        {
          path: 'core-languages',
          name: 'core-languages',
          component: VCoreLanguages,
        },
      ],
    },
  ],
});
