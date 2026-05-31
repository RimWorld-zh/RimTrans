import { App, app } from 'electron';
import { pth } from '@rimtrans/extractor';
import { createStates } from './utils';
import { initServices } from './services';
import { RimTransWindowOptions, createRimTransWindow } from './windows/rimtrans-window';

const isDevelopment = process.env.NODE_ENV === 'development';

export function createApp(): App {
  const states = createStates();

  const url = isDevelopment
    ? `http://localhost:9421/`
    : `file://${pth.join(__dirname, '..', 'renderer', 'index.html')}`;
  const options: RimTransWindowOptions = {
    url,
    hash: '/',
  };

  const onReady = async (): Promise<void> => {
    await states.loadStates();
    initServices(states);
    const win = await createRimTransWindow(states, options);
  };
  const onQuit = async (): Promise<void> => {
    await states.saveStates();
  };

  app.on('ready', onReady);
  app.on('quit', onQuit);

  return app;
}
