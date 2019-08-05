import ChildProcess from 'child_process';
import * as io from '@rimtrans/io';
import { FOLDER_NAME_MODS, ModMetaData } from '@rimtrans/extractor';
import { States } from '../../utils/states';
import { createSlaverMain } from '../../utils/slaver';
import { ModMetaDataSlaver } from './slaver';

declare module '../../utils/ipc' {
  interface IpcTypeMap {
    'mod-meta-data': ['local' | 'steam' | string[], Record<string, ModMetaData>];
  }
}

export function init(states: States): void {
  const { ipc } = states;

  ipc.addRequestHandler('mod-meta-data', async (e, data) => {
    return new Promise<Record<string, ModMetaData>>((resolve, reject) => {
      const slaver = createSlaverMain<ModMetaDataSlaver>(io.join(__dirname, 'slaver'));

      slaver.childProcess.on('error', error => reject(error));

      slaver.addListener('request', mods => {
        resolve(mods);
        slaver.childProcess.kill();
      });

      const genre = Array.isArray(data) ? 'files' : 'directories';
      const paths: string[] = (Array.isArray(data) && data) || [];
      if (data === 'local') {
        paths.push(io.join(states.settings.get().directoryRimWorld, FOLDER_NAME_MODS));
      }
      if (data === 'steam') {
        paths.push(states.settings.get().directoryWorkshop);
      }

      slaver.send('request', { genre, paths });
    });
  });
}
