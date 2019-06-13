import { genPathResolve } from '@huiji/shared-utils';
import { getFiles } from './io';

const resolvePath = genPathResolve(__dirname, '..', '..');

describe('io', () => {
  test('getFiles', async () => {
    const files = await getFiles(['*.ts'], { cwd: __dirname });
    expect(files.includes('io.ts')).toBe(true);
  });
});
