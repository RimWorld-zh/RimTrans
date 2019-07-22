import { app, BrowserWindow, Menu, dialog } from 'electron';
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

async function createWindow(): Promise<void> {
  let {
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

  if (maximized) {
    browserWindow.maximize();
  }

  const onResize = (): void => {
    if (browserWindow.isMaximized()) {
      return;
    }
    [width, height] = browserWindow.getSize();
    [x, y] = browserWindow.getPosition();
  };
  browserWindow.on('move', onResize);
  browserWindow.on('resize', onResize);
  browserWindow.on('maximize', onResize);
  browserWindow.on('restore', () => {
    browserWindow.setSize(width as number, height as number);
    browserWindow.setPosition(x as number, y as number);
  });
  browserWindow.once('close', () => {
    maximized = browserWindow.isMaximized();
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
  });

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
