import { pth, fse, globby, FOLDER_NAME_MODS, ModMetaData } from '@rimtrans/extractor';
import { States } from '../../utils/states';
import { createSlaverMain } from '../../utils/slaver';
import { ModMetaDataSlaver } from './models';

export function initModMetaDataHandler(states: States): void {
  const { ipc } = states;

  ipc.addRequestHandler('mod-meta-data', async (e, data) => {
    return new Promise<Record<string, ModMetaData>>((resolve, reject) => {
      const slaver = createSlaverMain<ModMetaDataSlaver>(pth.join(__dirname, 'slaver'));

      slaver.childProcess.on('error', error => reject(error));

      slaver.addListener('request', mods => {
        resolve(mods);
      });

      const genre = Array.isArray(data) ? 'files' : 'directories';
      const paths: string[] = (Array.isArray(data) && data) || [];
      if (data === 'local') {
        paths.push(pth.join(states.settings.get().directoryRimWorld, FOLDER_NAME_MODS));
      }
      if (data === 'steam') {
        paths.push(states.settings.get().directoryWorkshop);
      }

      slaver.send('request', { genre, paths });
    });
  });
}
