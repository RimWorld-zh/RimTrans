/**
 * Compress all package
 */
import globby from 'globby';
import compressing from 'compressing';
import { genPathResolve } from '@huiji/shared-utils';

const resolvePath = genPathResolve(__dirname, '..');

globby('rimtrans-*', { cwd: resolvePath('dist') }).then(async files =>
  Promise.all(
    files.map(file =>
      compressing.zip.compressFile(
        resolvePath('dist', file),
        resolvePath('dist', `${file}.zip`),
      ),
    ),
  ),
);
