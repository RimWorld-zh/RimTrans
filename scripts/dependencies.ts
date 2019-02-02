/**
 * Force upgrade all dependencies
 */
// tslint:disable:no-any no-unsafe-any
import fs from 'fs';
import globby from 'globby';
import execa from 'execa';
import { genPathResolve } from '@huiji/shared-utils';

const resolvePath = genPathResolve(__dirname, '..');

async function load(file: string): Promise<[string[], string[]]> {
  return new Promise<[string[], string[]]>((resolve, reject) => {
    fs.readFile(file, { encoding: 'utf-8' }, (error, content) => {
      if (error) {
        return reject(error);
      }
      const pkg = JSON.parse(content);

      return resolve([
        Object.keys(pkg.dependencies || {}),
        Object.keys(pkg.devDependencies || {}),
      ]);
    });
  });
}

(async () => {
  const packagesDir = resolvePath('packages');
  const pkgJsons = await globby([
    resolvePath('package.json'),
    resolvePath('packages', '*', 'package.json'),
    resolvePath('packages', '@*', '*', 'package.json'),
  ]);

  for (const file of pkgJsons) {
    const cwd = file.replace(/\/package.json$/, '');
    const [dependencies, devDependencies] = await load(file);

    if (dependencies.length > 0) {
      const command = [
        ['yarn', '--cwd', cwd, 'add', '-W'].join(' '),
        ...dependencies,
      ].join(' \\\n  ');
      console.info(command);
    }
    if (devDependencies.length > 0) {
      const command = [
        ['yarn', '--cwd', cwd, 'add', '-WD'].join(' '),
        ...devDependencies,
      ].join(' \\\n  ');
      console.info(command);
    }
  }

  console.info();
})();

console.log(process.env.npm_package_version);
