import { extract } from './index';

describe('extract', () => {
  test('test extract', async () => {
    const result = await extract();
    expect(result).toBe('extracted');
  });
});
