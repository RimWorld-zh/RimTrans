/* eslint-disable no-console,no-restricted-syntax,no-await-in-loop */
import fs from 'fs';
import { genPathResolve } from '@huiji/shared-utils';
import { ncp } from 'ncp';
import compressing from 'compressing';

const resolvePath = genPathResolve(__dirname, '..');

const platforms = ['win', 'linux', 'osx'];
const { npm_package_version: version } = process.env;

async function copyDir(src: string, dest: string): Promise<void> {
  return new Promise<void>((resolve, reject) =>
    ncp(src, dest, error => (error ? reject(error) : resolve())),
  );
}

const LIB = 'lib';
const EXECUTABLE = 'executable';
const REFLECTION = 'Reflection';

async function archive(): Promise<void> {
  for (const platform of platforms) {
    const folder = `rimtrans-v${version}-${platform}`;
    await fs.promises.mkdir(resolvePath(LIB, folder), { recursive: true });

    // copy executable
    await copyDir(resolvePath(EXECUTABLE, LIB, platform), resolvePath(LIB, folder));
    // copy reflection
    await copyDir(
      resolvePath(REFLECTION, LIB, platform),
      resolvePath(LIB, folder, REFLECTION),
    );

    // compress
    if (platform === 'win') {
      await compressing.zip.compressDir(
        resolvePath(LIB, folder),
        resolvePath(LIB, `${folder}.zip`),
      );
    } else {
      await compressing.tgz.compressDir(
        resolvePath(LIB, folder),
        resolvePath(LIB, `${folder}.tar.gz`),
      );
    }

    // TODO upload file to github release
    // https://developer.github.com/v3/repos/releases/
  }
}

archive().catch(console.error);
