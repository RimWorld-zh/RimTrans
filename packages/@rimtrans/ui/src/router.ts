import Vue from 'vue';
import VueRouter from 'vue-router';

Vue.use(VueRouter);

import VHome from './views/home';

/**
 * Router
 */
export default new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  routes: [
    {
      path: '/',
      name: 'home',
      component: VHome,
    },
  ],
});
