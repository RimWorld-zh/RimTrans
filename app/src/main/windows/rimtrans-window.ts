import { BrowserWindow, Rectangle, dialog, ipcMain } from 'electron';
import { States } from '../utils';

export interface RimTransWindowOptions {
  url: string;
  hash?: string;
}

export async function createRimTransWindow(
  states: States,
  options: RimTransWindowOptions,
): Promise<BrowserWindow> {
  const { settings, storage } = states;

  let {
    lastActiveWindowState: { maximized, width, height, x, y },
  } = storage.get();

  const win = new BrowserWindow({
    width: width || undefined,
    height: height || undefined,
    x: x || undefined,
    y: y || undefined,

    backgroundColor: '#fff',
    webPreferences: {
      nodeIntegration: true,
    },
  });

  if (maximized) {
    win.maximize();
  }

  const onBoundChange = (): void => {
    if (win.isMaximized()) {
      return;
    }
    ({ width, height, x, y } = win.getBounds());
  };
  const onRestore = (): void => {
    win.setSize(width as number, height as number);
    win.setPosition(x as number, y as number);
  };
  const onClose = (): void => {
    maximized = win.isMaximized();
    storage.set({ lastActiveWindowState: { maximized, width, height, x, y } });
  };

  win.on('move', onBoundChange);
  win.on('resize', onBoundChange);
  win.on('restore', onRestore);
  win.on('close', onClose);

  const { url, hash = '/' } = options;
  await win.loadURL(`${url}#${hash}`);

  return win;
}
