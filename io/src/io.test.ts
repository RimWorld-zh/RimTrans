import fs from 'fs';
import pth from 'path';
import * as io from './io';

describe('io', () => {
  test('join', () => {
    expect(io.join('a', 'b', 'c', 'd')).toBe(pth.join('a', 'b', 'c', 'd'));
  });

  test('basic', () => {
    expect(io.directoryName('/a/b/c/d/e')).toBe('/a/b/c/d');
    expect(io.fileName('/a/b/c')).toBe('c');
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
      io.search(['*.ts'], {
        cwd: io.join(__dirname, 'mock'),
      }),
    ).rejects.toThrowError(/ENOENT/);
  });

  test('exists', async () => {
    expect(await io.fileExists(io.join(__dirname, 'Mock', 'mock.txt'))).toBe(false);
    expect(await io.fileExists(__filename)).toBe(true);
    expect(await io.fileExists(`${__filename}.mock`)).toBe(false);
    expect(await io.fileExists(io.join(__dirname, 'mock', 'mock', 'mock.txt'))).toBe(
      false,
    );

    expect(await io.directoryExists(__dirname)).toBe(true);
    expect(await io.directoryExists(io.join(__dirname, 'mock'))).toBe(false);
    expect(await io.directoryExists(io.join(__dirname, 'mock', 'mock'))).toBe(false);
  });

  test('several', async () => {
    const dir = io.join(__dirname, '.tmp');
    const subDir = io.join(__dirname, '.tmp', 'mock');
    const file = io.join(__dirname, '.tmp', 'mock', 'mock.txt');
    const content = 'mocking bird';

    try {
      await io.copy(subDir, io.join(subDir, '1'));
      expect(false).toBeTruthy();
    } catch (error) {
      expect(error).toBeTruthy();
    }
    try {
      await io.copy(io.join(__dirname, 'io.ts'), io.join(dir, 'io.ts'));
      expect(false).toBeTruthy();
    } catch (error) {
      expect(error).toBeTruthy();
    }

    await io.createDirectory(dir);
    await io.copy(io.join(__dirname, 'io.ts'), io.join(dir, 'io.ts'));

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

    const typePackage = await io.load(io.join(__dirname, '..', 'package.json'));
    expect(typePackage.name).toBe('@rimtrans/io');
  });
});
