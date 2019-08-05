import Vue, { AsyncComponent } from 'vue';
import VueRouter from 'vue-router';

const placeHolder: AsyncComponent = async () =>
  import(/* webpackChunkName: "v-dev-tools" */ './views/dev-tools/place-holder');

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
            redirect: 'language',
          },
          {
            path: 'language',
            name: 'settings-language',
            component: async () =>
              import(/* webpackChunkName: "v-settings" */ './views/settings/language'),
          },
          {
            path: 'features',
            name: 'settings-features',
            component: async () =>
              import(/* webpackChunkName: "v-settings" */ './views/settings/features'),
          },
          {
            path: 'ui',
            name: 'settings-ui',
            component: async () =>
              import(/* webpackChunkName: "v-settings" */ './views/settings/ui'),
          },
          {
            path: '*',
            redirect: 'language',
          },
        ],
      },

      {
        path: '/translator',
        name: 'translator',
        component: async () =>
          import(/* webpackChunkName: "v-translator" */ './views/translator/translator'),
      },
      {
        path: '/modder',
        name: 'modder',
        component: placeHolder,
      },

      // dev-tools
      {
        path: '/dev-tools/icons',
        name: 'dev-tools-icons',
        component: async () =>
          import(/* webpackChunkName: "v-dev-tools" */ './views/dev-tools/icons'),
      },
      {
        path: '/dev-tools/colors',
        name: 'dev-tools-colors',
        component: async () =>
          import(/* webpackChunkName: "v-dev-tools" */ './views/dev-tools/colors'),
      },
    ],
  });
}
