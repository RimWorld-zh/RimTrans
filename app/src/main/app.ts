import { App, app, Event as ElectronEvent } from 'electron';
import * as io from '@rimtrans/io';
import { createStates } from './utils';
import { RimTransWindowOptions, createRimTransWindow } from './windows/rimtrans-window';

const isDevelopment = process.env.NODE_ENV === 'development';

export function createApp(): App {
  const singleInstanceLock = app.requestSingleInstanceLock();
  if (!singleInstanceLock) {
    app.quit();
    return app;
  }

  const states = createStates();

  const url = isDevelopment
    ? `http://localhost:9421/`
    : `file://${io.join(__dirname, '..', 'renderer', 'index.html')}`;
  const options: RimTransWindowOptions = {
    url,
    hash: '/',
  };

  const onSecondInstance = async (
    event: ElectronEvent,
    argv: string[],
    workingDirectory: string,
  ): Promise<void> => {
    const win = await createRimTransWindow(states, options);
  };
  const onReady = async (): Promise<void> => {
    await states.loadStates();
    const win = await createRimTransWindow(states, options);
  };
  const onQuit = async (): Promise<void> => {
    await states.saveStates();
  };

  app.on('second-instance', onSecondInstance);
  app.on('ready', onReady);
  app.on('quit', onQuit);

  return app;
}
