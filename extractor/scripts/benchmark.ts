import {
  pathCore,
  pathTestMods,
  pathsTypePackage,
  outputExtractor,
} from '../src/utils.test';
import { pth, fse, globby, ExtractorConfig, Extractor } from '../src';
import { createPrinter } from './printer';

const outputDirectory = `${outputExtractor}Benchmark`;
const outputResult = pth.join(outputDirectory, 'result.txt');

/* eslint-disable no-console */
function log(...args: string[]): void {
  fse.appendFileSync(outputResult, `${args.join(' ')}\n`);
  console.log(...args);
}
/* eslint-enable no-console */

function createExtractor(): Extractor {
  const extractor = new Extractor();

  extractor.emitter
    .addListener('info', (e, msg) => log(msg))
    .addListener('error', (e, msg) => log(msg))
    .addListener('warn', (e, msg) => log(msg));

  return extractor;
}

async function benchmark(): Promise<void> {
  await fse.remove(outputDirectory);
  await fse.mkdirp(outputDirectory);

  const languages = ['Template'];

  const modIds = await globby(['*'], { cwd: pathTestMods, onlyDirectories: true });
  modIds.sort();

  const configs = modIds.map<ExtractorConfig>(id => ({
    temp: '',
    typePackages: pathsTypePackage,
    modConfigs: [
      {
        path: pathCore,
        extract: false,
      },
      {
        path: pth.join(pathTestMods, id),
        extract: true,
        outputAsMod: true,
        outputPath: pth.join(outputDirectory, id),
      },
    ],
    languages,
    debugMode: true,
  }));
  configs.unshift({
    temp: '',
    typePackages: pathsTypePackage,
    modConfigs: [
      {
        path: pathCore,
        extract: true,
        outputAsMod: true,
        outputPath: pth.join(outputDirectory, 'Core'),
      },
    ],
    languages,
    debugMode: true,
  });

  log(`Start benchmark: the Core and ${modIds.length} mods.`);

  let totalCosts = 0;

  for (const cfg of configs) {
    const extractor = createExtractor();
    const modPath = (cfg.modConfigs[1] || cfg.modConfigs[0]).path;
    try {
      log(modPath);
      const start = Date.now();
      const mods = await extractor.extract(cfg);
      const mod = mods[1] || mods[0];
      const cost = Date.now() - start;
      totalCosts += cost;
      const message = [
        mod.meta.id.padEnd(16, ' '),
        `${cost}ms`.padEnd(16, ' '),
        mod.meta.name,
      ].join('');
      log(message);
    } catch (error) {
      let message: string;
      if (error instanceof Error) {
        message = `\nError:\n${modPath}\n${error.message}\n${error.stack}\n`;
      } else {
        message = `\nError:\n${modPath}\n${error}\n`;
      }
      log(message);
    }
  }

  log('');
  log(`Total costs: ${totalCosts}`);
}

benchmark();
