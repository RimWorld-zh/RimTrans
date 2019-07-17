import './renderer.scss';

import { createApp } from './app';

async function setup(): Promise<void> {
  const { app } = createApp();

  app.$mount('#app');
}

setup();
