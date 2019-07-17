import Vue from 'vue';
import VueRouter from 'vue-router';

export function createRouter(): VueRouter {
  return new VueRouter({
    mode: 'hash',
    base: process.env.BASE_URL,
    routes: [
      {
        path: '/',
        name: 'welcome',
        component: async () =>
          import(/* webpackChunkName: "welcome" */ './views/welcome'),
      },
    ],
  });
}
