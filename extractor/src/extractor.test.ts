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
  let configCore: ExtractorConfig;
  let configCoreOutput: ExtractorConfig;

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
          outputPath: io.join(outputExtractor, 'Core'),
        },
      ],
      languages,
      debugMode: true,
    };
  });

  test('Core', async () => {
    await Extractor.extract(configCore);
  });

  jest.retryTimes(3);
  test('Core Output', async () => {
    try {
      await Extractor.extract(configCoreOutput);
    } catch (error) {
      console.log(error);
      console.log(error.stack);
      expect(true).toBe(false);
    }
  });

  jest.retryTimes(3);
  test('Core Output Brand New', async () => {
    try {
      await Extractor.extract({
        ...configCoreOutput,
        brandNewMode: true,
      });
    } catch (error) {
      console.log(error);
      console.log(error.stack);
      expect(true).toBe(false);
    }
  });

  test('Mods', async () => {
    for (const sln of configs.slice(0, 3)) {
      await Extractor.extract(sln);
    }
  });
});
