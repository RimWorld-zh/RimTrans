/* eslint-disable no-console */
import * as io from '@rimtrans/io';
import { ExtractorConfig, Extractor } from '../src/extractor';
import {
  pathCore,
  pathTestMods,
  pathsTypePackage,
  outputExtractor,
  outputBenchmark,
} from '../src/utils.test';

async function benchmark(): Promise<void> {
  const result: string[] = [];

  const outputDirectory = `${outputExtractor}Benchmark`;
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

  console.log(`start benchmark: the Core and ${modIds.length} mods.`);

  let totalCosts = 0;

  for (const cfg of configs) {
    const modPath = (cfg.modConfigs[1] || cfg.modConfigs[0]).path;
    try {
      console.log(modPath);
      const start = Date.now();
      const mods = await Extractor.extract(cfg);
      const mod = mods[1] || mods[0];
      const cost = Date.now() - start;
      totalCosts += cost;
      const message = [
        mod.identify.padEnd(16, ' '),
        `${cost}ms`.padEnd(16, ' '),
        mod.meta.name,
      ].join('');
      console.log(message);
      result.push(message);
    } catch (error) {
      let message: string;
      if (error instanceof Error) {
        message = `\nError:\n${modPath}\n${error.message}\n${error.stack}\n`;
      } else {
        message = `\nError:\n${modPath}\n${error}\n`;
      }
      console.log(message);
      result.push(message);
    }
  }

  const lastMessage = `Total costs: ${totalCosts}`;
  result.push(lastMessage);
  io.save(outputBenchmark, result.join('\n'));
}

benchmark();
