/**
 * Update
 */
import fs, { mkdir } from 'fs';
import pth from 'path';
import chalk, { Chalk } from 'chalk';
import globby from 'globby';

import { genPathResolve } from '@huiji/shared-utils';

import io from '@rimtrans/io';

import { languageInfos } from '../src/languages';

const resolvePath = genPathResolve(__dirname, '..');

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

async function copy(src: string, dest: string, patterns: string[]): Promise<void> {
  const resolveSrc = genPathResolve(src);
  const resolveDest = genPathResolve(dest);

  const files = await globby(patterns, { cwd: src });

  files.sort();

  await Promise.all(
    [...new Set(files.map(f => pth.dirname(f)))].map(async dir =>
      io.createDirectory(resolveDest(dir)),
    ),
  );

  await Promise.all(
    files.map(async f => {
      if (/\.(md|xml|txt)$/.test(f)) {
        await io.save(resolveDest(f), await io.load(resolveSrc(f), true));
      } else {
        await io.copy(resolveSrc(f), resolveDest(f));
      }
    }),
  );

  await io.save(
    resolveDest('manifest.json'),
    JSON.stringify(
      {
        files,
      },
      undefined,
      '  ',
    ),
  );
}

(async () => {
  const [gamePath] = process.argv.slice(2);

  if (!gamePath) {
    return log.error('Please input path to RimWorld.');
  }
  if (!fs.existsSync(gamePath)) {
    return log.error(`No such directory: ${gamePath}`);
  }
  if (!fs.lstatSync(gamePath).isDirectory()) {
    return log.error(`Not a directory: ${gamePath}`);
  }

  const resolveGame = genPathResolve(gamePath);

  const version = (await io.load(resolveGame('Version.txt'))).trim().split(' ')[0];
  await io.save(
    resolvePath('src/version.ts'),
    `// tslint:disable\nexport default '${version}';\n`,
  );

  await Promise.all(
    ['About', 'Defs', 'Languages/English'].map(async dir => io.remove(dir)),
  );
  await copy(resolveGame('Mods/Core'), resolvePath('.'), [
    'About/**/*',
    'Defs/**/*.xml',
    'Languages/English/LangIcon.png',
    'Languages/English/**/*.xml',
    'Languages/English/**/*.txt',
  ]);

  await io.save(
    resolvePath('src', 'version.ts'),
    `// tslint:disable\nexport default '${version}';\n`,
  );
  log.success("Copied Core's 'About', 'Defs' and 'Languages/English‘");
})();
