import { getCoreFiles } from './index';

describe('rimworld-core', () => {
  test('test getCoreFiles', async () => {
    const { defs, defInjected, keyed, strings } = await getCoreFiles();
    expect(Object.keys(defs).length).toBe(413);
    expect(Object.keys(defInjected).length).toBe(2);
    expect(Object.keys(keyed).length).toBe(27);
    expect(Object.keys(strings).length).toBe(75);
  });
});
