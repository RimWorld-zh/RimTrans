import { getCoreTypeInfo } from './index';

describe('type-info', () => {
  test('test getCoreTypeInfo', async () => {
    const { classes } = await getCoreTypeInfo();
    expect(classes.filter(c => c.baseClass === 'Def').length).toBe(115);
  });
});
