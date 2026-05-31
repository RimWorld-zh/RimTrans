import pth from 'path';
import fse from 'fs-extra';
import globby from 'globby';

import { pathEnglishStrings, pathsStrings, outputStrings } from './utils.test';

import { ExtractorEventEmitter } from './extractor-event-emitter';
import { StringsFileExtractor } from './strings-file';

describe('string-file', () => {
  const emitter = new ExtractorEventEmitter();
  const stringsFileExtractor = new StringsFileExtractor(emitter);

  beforeAll(async () => {
    await fse.remove(outputStrings);
  });

  let [originMap, oldMap, mockMap, newMap] = [] as Record<string, string>[];

  test('load', async () => {
    [originMap, oldMap, mockMap] = await Promise.all(
      [pathEnglishStrings, ...pathsStrings].map(path => stringsFileExtractor.load(path)),
    );

    expect(Object.keys(mockMap).length).toBe(0);
  });

  test('merge', () => {
    newMap = stringsFileExtractor.merge(originMap, oldMap);
  });

  test('save', async () => {
    await stringsFileExtractor.save(outputStrings, newMap);
  });
});
