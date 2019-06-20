import * as io from '@rimtrans/io';
import { pathCore, pathTestMods, pathsTypePackage, outputExtractor } from './utils.test';
import { ExtractorSolution, extract } from './extractor';

describe('extractor', () => {
  let solutions: ExtractorSolution[];

  beforeAll(async () => {
    await io.deleteFileOrDirectory(outputExtractor);
    await io.createDirectory(outputExtractor);

    const languages = ['Template'];
    const outputDirectory = `${outputExtractor}Benchmark`;

    const modIds = await io.search(['*'], { cwd: pathTestMods, onlyDirectories: true });
    solutions = modIds.map<ExtractorSolution>(id => ({
      typePackages: pathsTypePackage,
      modPaths: [pathCore, io.join(pathTestMods, id)],
      enabledMods: [false, true],
      languages,
      outputDirectory,
    }));
  });

  test('Core', async () => {
    const sln: ExtractorSolution = {
      typePackages: pathsTypePackage,
      modPaths: [pathCore],
      enabledMods: [true],
      languages: ['Template'],
      outputDirectory: outputExtractor,
    };

    await extract(sln);
  });

  test('Mods', async () => {
    for (const sln of solutions.slice(0, 1)) {
      await extract(sln);
    }
  });
});
