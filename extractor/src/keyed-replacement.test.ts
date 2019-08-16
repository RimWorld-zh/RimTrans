import pth from 'path';
import fse from 'fs-extra';
import globby from 'globby';

import {
  pathEnglishKeyed,
  pathsKeyed,
  outputKeyed,
  outputKeyedEnglish,
  outputKeyedOld,
  outputKeyedNew,
} from './utils.test';

import { ExtractorEventEmitter } from './extractor-event-emitter';
import { KeyedReplacementMap, KeyedReplacementExtractor } from './keyed-replacement';

describe('keyed-replacement', () => {
  const emitter = new ExtractorEventEmitter();
  const keyedReplacementExtractor = new KeyedReplacementExtractor(emitter);

  let keyedMapEnglish: KeyedReplacementMap;
  let keyedMaps: KeyedReplacementMap[];
  let mergedMap: KeyedReplacementMap;

  test('load', async () => {
    [keyedMapEnglish, ...keyedMaps] = await Promise.all(
      [pathEnglishKeyed, ...pathsKeyed].map(path => keyedReplacementExtractor.load(path)),
    );

    expect(Array.isArray(keyedMapEnglish['Alerts.xml'])).toBe(true);
    expect(
      keyedMapEnglish['Alerts.xml'].some(
        k =>
          typeof k === 'object' &&
          k.key === 'BreakRiskMinor' &&
          k.origin === '' &&
          k.translation === 'Minor break risk',
      ),
    );

    // for check duplicated
    keyedMaps[0]['ZMocks1.xml'] = [
      'a comment',
      {
        key: 'WorkTagNone',
        origin: 'none',
        translation: 'TODO',
      },
    ];

    expect(keyedMaps.length).toBe(2);
    expect(Object.keys(keyedMaps[1]).length).toBe(0);
  });

  test('merge', async () => {
    mergedMap = await keyedReplacementExtractor.merge(keyedMapEnglish, keyedMaps[0]);
    await keyedReplacementExtractor.checkDuplicated([mergedMap]);

    expect(Array.isArray(mergedMap['Alerts.xml'])).toBe(true);
    expect(
      mergedMap['Alerts.xml'].some(
        k =>
          typeof k === 'object' &&
          k.key === 'BreakRiskMinor' &&
          k.origin === 'Minor break risk' &&
          k.translation === 'TODO',
      ),
    );

    await Promise.all([
      fse.outputJSON(outputKeyedEnglish, keyedMapEnglish, { spaces: 2 }),
      fse.outputJSON(outputKeyedOld, keyedMaps[0], { spaces: 2 }),
      fse.outputJSON(outputKeyedNew, mergedMap, { spaces: 2 }),
    ]);
  });

  test('save', async () => {
    await fse.remove(outputKeyed);
    await keyedReplacementExtractor.save(outputKeyed, mergedMap);

    expect(await fse.pathExists(outputKeyed)).toBe(true);
    expect(await fse.pathExists(pth.join(outputKeyed, 'Alerts.xml'))).toBe(true);
  });
});
