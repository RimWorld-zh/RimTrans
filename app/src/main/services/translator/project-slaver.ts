import chokidar from 'chokidar';
import { pth, fse, globby } from '@rimtrans/extractor';
import { EXT_NAME_TRANSLATOR_PROJECT } from '../../utils/constants';
import { setupFSWatchSlaver } from '../fs-watcher';
import { TranslatorProject, TranslatorProjectMetaData } from './models';

function setup(): void {
  const directory = process.argv.slice(2)[0];
  if (!directory) {
    throw new Error('Missing the argument directory.');
  }
  const slaver = setupFSWatchSlaver<TranslatorProject, TranslatorProjectMetaData>({
    directory,

    // watch
    watchOptions: {},
    save: (path, data) => fse.outputJSON(path, data, { spaces: 2 }),
    load: path => fse.readJSON(path),

    // search
    searchGlob: [`*${EXT_NAME_TRANSLATOR_PROJECT}`],
    searchGlobbyOptions: {
      caseSensitiveMatch: false,
      onlyFiles: true,
    },
    searchResolver: path =>
      fse.readJSON(path).then((data: TranslatorProject) => data.meta),
  });
}

setup();
