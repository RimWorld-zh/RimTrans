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
      {
        path: '/settings',
        component: async () =>
          import(/* webpackChunkName: "v-settings" */ './views/settings/settings'),
        children: [
          {
            path: '',
            name: 'settings',
            redirect: 'languages',
          },
          {
            path: 'languages',
            name: 'settings-languages',
            component: async () =>
              import(/* webpackChunkName: "v-settings" */ './views/settings/languages'),
          },
        ],
      },

      // Debug
      {
        path: '/debug/icons',
        name: 'debug-icons',
        component: async () =>
          import(/* webpackChunkName: "v-debug" */ './views/debug/icons'),
      },
    ],
  });
}
