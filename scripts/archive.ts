/* eslint-disable no-console,no-restricted-syntax,no-await-in-loop */
import { genPathResolve } from '@huiji/shared-utils';
import compressing from 'compressing';
import * as io from '@rimtrans/io';

const resolvePath = genPathResolve(__dirname, '..');

const PLATFORMS = ['win', 'linux', 'osx'];
const { npm_package_version: version } = process.env;

const DIST = 'dist';
const EXECUTABLE = 'executable';
const CORE = 'Core';
const REFLECTION = 'Reflection';
const TYPE_INFO = 'type-package.json';
const TYPE_INFO_FIX = 'type-package-fix.json';

async function archive(): Promise<void> {
  for (const platform of PLATFORMS) {
    console.log(`version: ${version}, platform: ${platform}`);
    const folder = `rimtrans-v${version}-${platform}`;
    await io.createDirectory(resolvePath(DIST, folder));
    {
      // Copy executable
      const source = resolvePath(EXECUTABLE, DIST, platform);
      const target = resolvePath(DIST, folder);
      await io.copy(source, target);
    }
    {
      // Copy Reflection
      const source = resolvePath(REFLECTION, DIST, platform);
      const target = resolvePath(DIST, folder, REFLECTION);
      await io.copy(source, target);
    }
    {
      // Copy Core
      const source = resolvePath(CORE);
      const target = resolvePath(DIST, folder, CORE);
      await io.copy(source, target);
    }
    {
      // Copy type-package.json
      const source = resolvePath(REFLECTION, TYPE_INFO);
      const target = resolvePath(DIST, folder, TYPE_INFO);
      await io.copy(source, target);
    }
    {
      // Copy type-package-fix.json
      const source = resolvePath(REFLECTION, TYPE_INFO_FIX);
      const target = resolvePath(DIST, folder, TYPE_INFO_FIX);
      await io.copy(source, target);
    }

    // Compress
    if (platform === 'win') {
      await compressing.zip.compressDir(
        resolvePath(DIST, folder),
        resolvePath(DIST, `${folder}.zip`),
      );
    } else {
      await compressing.tgz.compressDir(
        resolvePath(DIST, folder),
        resolvePath(DIST, `${folder}.tar.gz`),
      );
    }

    // TODO upload file to github release
    // https://developer.github.com/v3/repos/releases/
  }
}

archive();
