import * as io from '@rimtrans/io';
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
      io.save(outputKeyedEnglish, JSON.stringify(keyedMapEnglish, undefined, '  ')),
      io.save(outputKeyedOld, JSON.stringify(keyedMaps[0], undefined, '  ')),
      io.save(outputKeyedNew, JSON.stringify(mergedMap, undefined, '  ')),
    ]);
  });

  test('save', async () => {
    await io.deleteFileOrDirectory(outputKeyed);
    await keyedReplacementExtractor.save(outputKeyed, mergedMap);

    expect(await io.directoryExists(outputKeyed)).toBe(true);
    expect(await io.fileExists(io.join(outputKeyed, 'Alerts.xml'))).toBe(true);
  });
});
