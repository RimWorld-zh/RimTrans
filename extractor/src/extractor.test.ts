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
  const extractor = new Extractor();
  let configs: ExtractorConfig[];
  let configCore: ExtractorConfig;
  let configCoreOutput: ExtractorConfig;
  let configCoreBrandNew: ExtractorConfig;

  beforeAll(async () => {
    await io.deleteFileOrDirectory(outputExtractor);
    await io.createDirectory(outputExtractor);

    const languages = ['Template', 'Mocking'];

    const modIds = await io.search(['*'], { cwd: pathTestMods, onlyDirectories: true });
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
          outputPath: io.join(outputExtractor, id),
        },
      ],
      languages,
    }));

    configCore = {
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

    configCoreOutput = {
      temp: './.temp',
      typePackages: pathsTypePackage,
      modConfigs: [
        {
          path: pathCore,
          extract: true,
          outputAsMod: true,
          outputPath: io.join(outputExtractor, 'CoreOutput'),
        },
      ],
      languages,
      debugMode: true,
    };

    configCoreBrandNew = {
      temp: './.temp',
      typePackages: pathsTypePackage,
      modConfigs: [
        {
          path: pathCore,
          extract: true,
          outputAsMod: true,
          outputPath: io.join(outputExtractor, 'CoreBrandNew'),
        },
      ],
      languages,
      brandNewMode: true,
      debugMode: true,
    };
  });

  test('Core', async () => {
    await extractor.extract(configCore);
  });

  test('Core Output', async () => {
    await extractor.extract(configCoreOutput);
    await extractor.extract(configCoreOutput);
  });

  test('Core Output Brand New', async () => {
    await extractor.extract(configCoreBrandNew);
    await extractor.extract(configCoreBrandNew);
  });

  test('Mods', async () => {
    for (const cfg of configs.slice(0, 3)) {
      await extractor.extract(cfg);
    }
  });
});
