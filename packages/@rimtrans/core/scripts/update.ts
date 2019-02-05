/**
 * Update
 */
import fs, { mkdir } from 'fs';
import pth from 'path';
import chalk, { Chalk } from 'chalk';
import execa from 'execa';
import globby from 'globby';
import mkdirp from 'mkdirp';
import rimraf from 'rimraf';
import { genPathResolve } from '@huiji/shared-utils';
import io from '@rimtrans/io';

const loggerMap = {
  info: chalk.cyanBright,
  success: chalk.greenBright,
  warning: chalk.yellowBright,
  error: chalk.redBright,
};

const log: Record<keyof typeof loggerMap, (msg: string) => void> = {} as any;

Object.entries(loggerMap).forEach(
  ([key, color]) =>
    (log[key as keyof typeof loggerMap] = (msg: string) =>
      // tslint:disable-next-line:no-console
      console.log(color(msg))),
);

(async () => {
  const [corePath] = process.argv.slice(2);

  if (!corePath) {
    return log.error('Please input path to RimWorld Core.');
  }
  if (!fs.existsSync(corePath)) {
    return log.error(`No such directory: ${corePath}`);
  }
  if (!fs.lstatSync(corePath).isDirectory()) {
    return log.error(`Not a directory: ${corePath}`);
  }

  const resolvePath = genPathResolve(__dirname, '..');
  const resolveSrc = genPathResolve(corePath);
  const files = await globby(['About/**/*', 'Defs/**/*', 'Languages/English/**/*'], {
    cwd: corePath,
  });

  await Promise.all(
    [...new Set(files.map(f => pth.dirname(f)))].map(async dir =>
      io.createDirectory(resolvePath(dir)),
    ),
  );

  await Promise.all(
    files.map(async f => {
      if (/\.(md|xml|txt|meta~alpha14)$/.test(f)) {
        const content = await io.load(resolveSrc(f));
        await io.save(resolvePath(f), content);
      } else {
        await io.copy(resolveSrc(f), resolvePath(f));
      }
    }),
  );

  log.success(
    `Update Core files for About, Defs and Language English, total: ${files.length}`,
  );
})();
