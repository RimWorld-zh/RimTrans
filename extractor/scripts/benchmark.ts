/* eslint-disable no-console */
import * as io from '@rimtrans/io';
import { ExtractorSolution, extract } from '../src/extractor';
import {
  pathCore,
  pathTestMods,
  pathsTypePackage,
  outputExtractor,
  outputBenchmark,
} from '../src/utils.test';

async function benchmark(): Promise<void> {
  const result: string[] = [];

  await io.deleteFileOrDirectory(outputExtractor);
  await io.createDirectory(outputExtractor);

  const languages = ['Template'];
  const outputDirectory = `${outputExtractor}Benchmark`;

  const modIds = await io.search(['*'], { cwd: pathTestMods, onlyDirectories: true });
  const solutions = modIds.map<ExtractorSolution>(id => ({
    typePackages: pathsTypePackage,
    modPaths: [pathCore, io.join(pathTestMods, id)],
    enabledMods: [false, true],
    languages,
    outputDirectory,
  }));
  solutions.unshift({
    typePackages: pathsTypePackage,
    modPaths: [pathCore],
    enabledMods: [true],
    languages,
    outputDirectory,
  });

  console.log(`start benchmark: the Core and ${modIds.length} mods.`);

  let totalCosts = 0;

  for (const sln of solutions) {
    try {
      const start = Date.now();
      const mods = await extract(sln);
      const mod = mods[1] || mods[0];
      const cost = Date.now() - start;
      totalCosts += cost;
      const message = `${mod.identify.padEnd(20, ' ')}${cost}ms    ${mod.meta.name}`;
      console.log(message);
      result.push(message);
    } catch (error) {
      if (error instanceof Error) {
        result.push(`\n${sln.modPaths.join('\n')}\n${error.message}\n${error.stack}\n`);
      } else {
        result.push(`\n${sln.modPaths.join('\n')}\n${error}\n`);
      }
    }
  }

  const lastMessage = `Total costs: ${totalCosts}`;
  result.push(lastMessage);
  io.save(outputBenchmark, result.join('\n'));
}

benchmark();
