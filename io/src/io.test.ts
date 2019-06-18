import fs from 'fs';
import pth from 'path';
import * as io from './io';

describe('io', () => {
  test('join', () => {
    expect(io.join('a', 'b', 'c', 'd')).toBe(pth.join('a', 'b', 'c', 'd'));
  });

  test('basic', () => {
    expect(io.directoryName('/a/b/c/d/e')).toBe('/a/b/c/d');
    expect(io.fileName('/a/b/c.d')).toBe('c');
    expect(io.fileName('/a/b/c.d', false)).toBe('c.d');
    expect(io.extensionName('/a/b/c.d')).toBe('.d');
  });

  test('search', async () => {
    expect(
      await io
        .search(['*.ts'], { cwd: __dirname })
        .then(files => files.includes('io.ts') && files.includes('io.test.ts')),
    ).toBe(true);

    expect(
      io.search(['*.ts'], { cwd: pth.join(__dirname, 'mock') }),
    ).rejects.toThrowError(/ENOENT/);
  });

  test('exists', async () => {
    expect(await io.fileExists(__filename)).toBe(true);
    expect(await io.fileExists(`${__filename}.mock`)).toBe(false);
    expect(await io.fileExists(pth.join(__dirname, 'mock', 'mock', 'mock.txt'))).toBe(
      false,
    );

    expect(await io.directoryExists(__dirname)).toBe(true);
    expect(await io.directoryExists(pth.join(__dirname, 'mock'))).toBe(false);
    expect(await io.directoryExists(pth.join(__dirname, 'mock', 'mock'))).toBe(false);
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
