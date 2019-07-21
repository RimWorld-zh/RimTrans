import { app, BrowserWindow, Menu } from 'electron';
import * as io from '@rimtrans/io';
import * as utils from './utils';

const isDevelopment = process.env.NODE_ENV === 'development';

const { paths, browserWindowsSet, settings, storage, initStates } = utils.createStates();

async function setup(): Promise<void> {
  await initStates();
}

async function dispose(): Promise<void> {
  browserWindowsSet.clear();
  await Promise.all([settings, storage].map(wrapper => wrapper.save()));
}

function destroyWindow(browserWindow: BrowserWindow): void {
  const maximized = browserWindow.isMaximized();
  const [width, height] = browserWindow.getSize();
  const [x, y] = browserWindow.getPosition();

  storage.set(
    {
      lastActiveWindowState: {
        maximized,
        width,
        height,
        x,
        y,
      },
    },
    false,
  );

  browserWindowsSet.delete(browserWindow);
}

async function createWindow(): Promise<void> {
  const {
    lastActiveWindowState: { maximized, width, height, x, y },
  } = storage.get();

  const browserWindow = new BrowserWindow({
    width: width || undefined,
    height: height || undefined,
    x: x || undefined,
    y: y || undefined,

    backgroundColor: '#fff',
    webPreferences: {
      nodeIntegration: true,
    },
  });

  browserWindowsSet.add(browserWindow);

  browserWindow.once('close', () => destroyWindow(browserWindow));

  if (maximized) {
    browserWindow.maximize();
  }

  if (isDevelopment) {
    browserWindow.setAlwaysOnTop(true);
    await browserWindow.loadURL(`http://localhost:9421`);
  } else {
    await browserWindow.loadFile(io.join(__dirname, '..', 'renderer', 'index.html'));
  }
}

if (!isDevelopment) {
  Menu.setApplicationMenu(null);
}

app.on('ready', async () => {
  await setup();
  await createWindow();
});
app.on('quit', async () => {
  await dispose();
});
