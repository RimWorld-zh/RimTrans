import { resolve } from 'path';
import { app, BrowserWindow } from 'electron';

const isDevelopment = process.env.NODE_ENV === 'development';

export async function createWindow(): Promise<void> {
  const win = new BrowserWindow({
    width: 1440,
    height: 900,
    x: -1920,
    y: 0,
    alwaysOnTop: true,
  });

  win.maximize();

  const userData = app.getPath('userData');
  const appName = app.getName();

  if (isDevelopment) {
    await win.loadURL(`http://localhost:9421#/?appName=${appName}&userData=${userData}`);
  } else {
    await win.loadFile(resolve(__dirname, '..', 'renderer', 'index.html'));
  }
}

app.on('ready', createWindow);
