/**
 * Upgrade
 */

import { genPathResolve } from '@huiji/shared-utils';

import { download, unzip } from '@rimtrans/io';

import { languageInfos } from '../src/languages';

const resolvePath = genPathResolve(__dirname, '..');

(async () => {
  for (const info of languageInfos) {
    console.log(`Downloading ${info.name}...`);

    const url = `https://github.com/Ludeon/${info.repo}/archive/master.zip`;
    // const url = `https://codeload.github.com/Ludeon/${info.repo}/zip/master`;
    const file = resolvePath('.tmp', `${info.name}.zip`);
    await download(url, file);
    await unzip(file, resolvePath('.tmp'));
  }
})();
