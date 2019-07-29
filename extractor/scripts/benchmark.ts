import fs from 'fs';
import * as io from '@rimtrans/io';
import {
  pathCore,
  pathTestMods,
  pathsTypePackage,
  outputExtractor,
} from '../src/utils.test';
import { sleep } from '../src/extractor-event-emitter';
import { ExtractorConfig, Extractor } from '../src/extractor';
import { createPrinter } from './printer';

const outputDirectory = `${outputExtractor}Benchmark`;
const outputResult = io.join(outputDirectory, 'result.txt');

/* eslint-disable no-console */
function log(message: string): void {
  fs.appendFileSync(outputResult, `${message}\n`);
  console.log(message);
}
/* eslint-enable no-console */

function createExtractor(): Extractor {
  const extractor = new Extractor();

  extractor.emitter.addListener('error', (event, error) => {
    log(error);
  });

  return extractor;
}

async function benchmark(): Promise<void> {
  await io.deleteFileOrDirectory(outputDirectory);
  await io.createDirectory(outputDirectory);

  const languages = ['Template'];

  const modIds = await io.search(['*'], { cwd: pathTestMods, onlyDirectories: true });
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
        path: io.join(pathTestMods, id),
        extract: true,
        outputAsMod: true,
        outputPath: io.join(outputDirectory, id),
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
        outputPath: io.join(outputDirectory, 'Core'),
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
        mod.identify.padEnd(16, ' '),
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
