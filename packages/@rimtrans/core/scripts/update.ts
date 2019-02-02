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

const io = {
  async load(file: string): Promise<string> {
    return new Promise<string>((resolve, reject) =>
      fs.readFile(file, { encoding: 'utf-8' }, (error, content) =>
        error
          ? reject(error)
          : resolve(content.replace(/^\ufeff/g, '').replace(/\r\n/g, '\n')),
      ),
    );
  },
  async save(file: string, content: string): Promise<void> {
    return new Promise<void>((resolve, reject) =>
      fs.writeFile(file, content, { encoding: 'utf-8' }, error =>
        error ? reject(error) : resolve(),
      ),
    );
  },
  async copy(src: string, dest: string): Promise<void> {
    return new Promise<void>((resolve, reject) =>
      fs.copyFile(src, dest, error => (error ? reject(error) : resolve())),
    );
  },
  async mkdir(dir: string): Promise<void> {
    return new Promise<void>((resolve, reject) =>
      mkdirp(dir, error => (error ? reject(error) : resolve())),
    );
  },
};

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
      io.mkdir(resolvePath(dir)),
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
