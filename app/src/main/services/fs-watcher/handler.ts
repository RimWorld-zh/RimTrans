/* eslint-disable @typescript-eslint/no-explicit-any */
import { BrowserWindow } from 'electron';
import { IpcMain, createIpc } from '../../utils/ipc';
import { SlaverMain, createSlaverMain } from '../../utils/slaver';
import { FSWatchSlaver, FSWatchIpcTypeMap } from './models';

export interface FSWatchHandlerOptions {
  ipcNamespace: string;
  slaverPath: string;
  slaverArgs?: string[];
}

export function initFSWatchHandler<TWatch = string, TSearch = string>(
  options: FSWatchHandlerOptions,
): {
  ipc: IpcMain<FSWatchIpcTypeMap<TWatch, TSearch>>;
  slaver: SlaverMain<FSWatchSlaver<TWatch, TSearch>>;
} {
  const { ipcNamespace, slaverPath, slaverArgs = [] } = options;
  const ipc = createIpc<FSWatchIpcTypeMap<TWatch, TSearch>>(ipcNamespace);
  const slaver = createSlaverMain<FSWatchSlaver<TWatch, TSearch>>(slaverPath, slaverArgs);

  slaver.addListener('error', data => ipc.sendAll('error', { data }));

  ipc.on('add', (e, { data }) => slaver.send('add', data));
  slaver.addListener('add', data => ipc.sendAll('add', { data }));

  ipc.on('addDir', (e, { data }) => slaver.send('addDir', data));
  slaver.addListener('addDir', data => ipc.sendAll('addDir', { data }));

  ipc.addRequestHandler('read', (e, expectedPath) => {
    return new Promise<[string, TWatch]>((resolve, reject) => {
      const read = ([path, data]: [string, TWatch]): void => {
        if (path === expectedPath) {
          slaver.removeListener('read', read);
          resolve([path, data]);
        }
      };
      slaver.addListener('read', read);
    });
  });

  ipc.on('change', (e, { data }) => slaver.send('change', data));
  slaver.addListener('change', data => ipc.sendAll('change', { data }));

  ipc.on('unlink', (e, { data }) => slaver.send('unlink', data));
  slaver.addListener('unlink', data => ipc.sendAll('unlink', { data }));

  ipc.on('unlinkDir', (e, { data }) => slaver.send('unlinkDir', data));
  slaver.addListener('unlinkDir', data => ipc.sendAll('unlinkDir', { data }));

  ipc.addRequestHandler('search', e => {
    return new Promise<Record<string, TSearch>>((resolve, reject) => {
      const search = (result: Record<string, TSearch>): void => {
        slaver.removeListener('search', search);
        resolve(result);
      };
      slaver.addListener('search', search);
      slaver.send('search', undefined);
    });
  });

  return { ipc, slaver };
}
