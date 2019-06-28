import * as io from '@rimtrans/io';
import { pathEnglishStrings, pathsStrings, outputStrings } from './utils.test';
import { load, merge, save } from './strings-file';

describe('string-file', () => {
  beforeAll(async () => {
    await io.deleteFileOrDirectory(outputStrings);
  });

  let [originMap, oldMap, mockMap, newMap] = [] as Record<string, string>[];

  test('load', async () => {
    [[originMap], [oldMap, mockMap]] = await Promise.all([
      load([pathEnglishStrings]),
      load(pathsStrings),
    ]);

    expect(Object.keys(mockMap).length).toBe(0);
  });

  test('merge', () => {
    newMap = merge(originMap, oldMap);
  });

  test('save', async () => {
    await save(outputStrings, newMap);
  });
});
