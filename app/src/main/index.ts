import { app, BrowserWindow } from 'electron';

async function createWindow(): Promise<void> {
  const win = new BrowserWindow({
    width: 800,
    height: 800,
  });

  await win.loadURL('https://github.com/rimworld-zh/rimtrans');
}

app.on('ready', createWindow);
