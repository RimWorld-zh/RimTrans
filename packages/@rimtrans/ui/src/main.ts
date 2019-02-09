/**
 * Main
 */
// tslint:disable:no-import-side-effect
import './main.scss';
import Vue from 'vue';

// Font Awesome Icon
import { config, library } from '@fortawesome/fontawesome-svg-core';
import { fab } from '@fortawesome/free-brands-svg-icons';
import { far } from '@fortawesome/free-regular-svg-icons';
import { fas } from '@fortawesome/free-solid-svg-icons';
config.autoAddCss = false;
library.add(fab, far, fas);
import {
  FontAwesomeIcon as FaIcon,
  FontAwesomeLayers as FaLayers,
  FontAwesomeLayersText as FaLayersText,
} from '@fortawesome/vue-fontawesome';
Object.entries({ FaIcon, FaLayers, FaLayersText }).forEach(([name, comp]) =>
  Vue.component(name, comp),
);

// Void-UI
import VoidUI, { VoidUIOptions, VdTheme } from 'void-ui';
Vue.use<VoidUIOptions>(VoidUI);

import { wsc } from '@rimtrans/service';

import { PluginConfigs } from './utils/plugin-configs';

// Components
import * as allComponents from './components/all';
Object.entries(allComponents).forEach(([name, comp]) => Vue.component(name, comp));

import { VApp } from './views/app';
import { locale } from './i18n';
import { router } from './router';

wsc.ws.addEventListener('open', async () => {
  const vdTheme = new VdTheme({
    propsData: {
      theme: 'lite',
    },
  });
  const pluginConfigs = new PluginConfigs({
    locale,
    vdTheme,
  });
  Vue.use(pluginConfigs);
  await pluginConfigs.init();

  new Vue({
    locale,
    router,
    vdTheme,
    render: h => h(VApp),
  }).$mount('#app');
});
