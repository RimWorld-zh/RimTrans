import * as io from '@rimtrans/io';
import {
  resolvePath,
  pathCore,
  pathTestMods,
  pathsTypePackage,
  outputExtractor,
} from './utils.test';
import { ExtractorConfig, Extractor } from './extractor';

describe('extractor', () => {
  let configs: ExtractorConfig[];

  beforeAll(async () => {
    await io.deleteFileOrDirectory(outputExtractor);
    await io.createDirectory(outputExtractor);

    const languages = ['Template', 'Mocking'];
    const outputDirectory = `${outputExtractor}Benchmark`;

    const modIds = await io.search(['*'], { cwd: pathTestMods, onlyDirectories: true });
    /**
    
      modPaths: [pathCore, io.join(pathTestMods, id)],
      enabledMods: [false, true],
      languages,
      outputDirectory,
     */
    configs = modIds.map<ExtractorConfig>(id => ({
      temp: './.temp',
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
    }));
  });

  test('Core', async () => {
    const cfg: ExtractorConfig = {
      temp: './.temp',
      typePackages: pathsTypePackage,
      modConfigs: [
        {
          path: pathCore,
          extract: true,
        },
      ],
      languages: ['Template'],
    };

    await Extractor.extract(cfg);
  });

  test('Mods', async () => {
    for (const sln of configs.slice(0, 1)) {
      await Extractor.extract(sln);
    }
  });
});
