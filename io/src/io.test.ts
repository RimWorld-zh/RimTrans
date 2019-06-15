import fs from 'fs';
import pth from 'path';
import * as io from './io';

describe('io', () => {
  test('getFiles', async () => {
    const files = await io.getFiles(['*.ts'], { cwd: __dirname });
    expect(files.includes('io.ts')).toBe(true);
  });

  test('fileExists', async () => {
    expect(await io.fileExists(__filename)).toBe(true);
    expect(await io.fileExists(`${__filename}.mock`)).toBe(false);
  });

  test('several', async () => {
    const dir = pth.join(__dirname, '.tmp');
    const subDir = pth.join(__dirname, '.tmp', 'mock');
    const file = pth.join(__dirname, '.tmp', 'mock', 'mock.txt');
    const content = 'mocking bird';

    await io.deleteFileOrDirectory(dir);

    await io.createDirectory(subDir);
    expect(await io.directoryExists(subDir)).toBe(true);

    await io.deleteFileOrDirectory(dir);
    expect(await io.directoryExists(subDir)).toBe(false);

    await io.save(file, content);
    expect(await io.read(file)).toBe(content);

    await io.deleteFileOrDirectory(file);
    expect(await io.fileExists(file)).toBe(false);

    await io.save(file, content);
    expect(await io.read(file)).toBe(content);

    await io.deleteFileOrDirectory(dir);

    const typePackage = await io.load(
      pth.join(__dirname, '..', '..', 'Reflection', 'type-info.json'),
    );
    expect(typePackage.classes.length).toBeGreaterThan(0);
  });
});
