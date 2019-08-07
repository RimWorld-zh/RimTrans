import * as io from '@rimtrans/io';
import {
  resolvePath,
  pathCore,
  pathTestMods,
  pathsTypePackage,
  outputExtractor,
} from './utils.test';
import { WorkflowMap, setWorkflowMap } from './extractor-event-emitter';
import { ExtractorConfig, Extractor } from './extractor';

describe('extractor', () => {
  let configs: ExtractorConfig[];
  let configCore: ExtractorConfig;
  let configCoreOutput: ExtractorConfig;
  let configCoreBrandNew: ExtractorConfig;

  const createExtractor = (): [Extractor, () => WorkflowMap] => {
    const extractor = new Extractor();
    let map: WorkflowMap;
    extractor.emitter.addListener('workflowMap', (e, data) => (map = data));
    extractor.emitter.addListener('workflow', (e, payload) => {
      try {
        setWorkflowMap(map, payload, true);
      } catch (error) {
        console.log(payload);
        throw error;
      }
    });

    return [extractor, () => map];
  };

  const isResolved = (map: WorkflowMap): boolean => {
    let result = true;
    for (const [key, { status, children }] of Object.entries(map)) {
      if (status && status !== 'resolved') {
        return false;
      }
      result = result && isResolved(children);
    }
    return result;
  };

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
    const [extractor, getMap] = createExtractor();
    await extractor.extract(configCore);
    const map = getMap();
    if (!isResolved(map)) {
      console.log(JSON.stringify(map, undefined, '  '));
    }
  });

  test('Core Output', async () => {
    const [extractor, getMap] = createExtractor();
    await extractor.extract(configCoreOutput);
    await extractor.extract(configCoreOutput);
  });

  test('Core Output Brand New', async () => {
    const [extractor, getMap] = createExtractor();
    await extractor.extract(configCoreBrandNew);
    await extractor.extract(configCoreBrandNew);
  });

  test('Mods and Workflow', async () => {
    for (const cfg of configs.slice(0, 3)) {
      const [extractor, getMap] = createExtractor();
      await extractor.extract(cfg);
      const map = getMap();
      if (!isResolved(map)) {
        console.log(JSON.stringify(map, undefined, '  '));
      }
    }
  });
});
