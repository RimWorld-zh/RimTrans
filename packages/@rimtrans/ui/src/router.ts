import Vue from 'vue';
import VueRouter from 'vue-router';
Vue.use(VueRouter);

import { VHome } from './views/home';
import { VConfigs } from './views/configs/index';
import { VConfigsInterfaceLanguages } from './views/configs/interface-languages';
import { VConfigsApplication } from './views/configs/application';
import { VConfigsCoreLanguages } from './views/configs/core-languages';
import { VConfigsAbout } from './views/configs/about';

/**
 * Router
 */
export const router = new VueRouter({
  mode: 'history',
  base: '/',
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
          path: 'application',
          name: 'configs-application',
          component: VConfigsApplication,
        },
        {
          path: 'interface-languages',
          name: 'configs-interface-languages',
          component: VConfigsInterfaceLanguages,
        },
        {
          path: 'core-languages',
          name: 'configs-core-languages',
          component: VConfigsCoreLanguages,
        },
        {
          path: 'about',
          name: 'configs-about',
          component: VConfigsAbout,
        },
      ],
    },
  ],
});
