import { app, BrowserWindow } from 'electron';

async function createWindow(): Promise<void> {
  const win = new BrowserWindow({
    width: 800,
    height: 600,
    webPreferences: {
      nodeIntegration: true,
    },
  });

  await win.loadURL('electron-builder');
}

app.on('ready', createWindow);
