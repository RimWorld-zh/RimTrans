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
      await io
        .search(['*.TS'], { cwd: __dirname, caseSensitiveMatch: false })
        .then(files => files.includes('io.ts') && files.includes('io.test.ts')),
    ).toBe(true);

    expect(
      await io
        .search(['src/*.ts'])
        .then(files => files.includes('src/io.ts') && files.includes('src/io.test.ts')),
    ).toBe(true);

    expect(
      await io
        .search(['src/*.TS'], { caseSensitiveMatch: false })
        .then(files => files.includes('src/io.ts') && files.includes('src/io.test.ts')),
    ).toBe(true);

    expect(await io.search(['src/*.TS'])).toEqual([]);

    expect(await io.search(['*.ts'], { cwd: io.join(__dirname, 'mock') })).toEqual([]);

    expect(await io.search(['*'], { cwd: io.join(__dirname, 'foobar') })).toEqual([]);
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
    const dir = io.join(__dirname, '.tmp', 'test');
    await io.deleteFileOrDirectory(dir);

    let error: Error | undefined;
    try {
      await io.copy(io.join(__dirname, 'foobar', 'io.ts'), io.join(dir, 'io.ts'));
    } catch (e) {
      error = e;
    }
    expect(error).toBeTruthy();

    await io.copy(io.join(__dirname, 'io.ts'), io.join(dir, 'io.ts'));
    expect(await io.fileExists(io.join(dir, 'io.ts'))).toBe(true);

    await io.copy(io.join(__dirname, 'io.ts'), io.join(dir, 'foobar', 'io.ts'));
    expect(await io.fileExists(io.join(dir, 'foobar', 'io.ts'))).toBe(true);

    await io.deleteFileOrDirectory(dir);

    const subDir = io.join(__dirname, '.tmp', 'mock');
    const file = io.join(__dirname, '.tmp', 'mock', 'mock.txt');
    const content = 'mocking bird';

    await io.createDirectory(subDir);
    expect(await io.directoryExists(subDir)).toBe(true);

    await io.deleteFileOrDirectory(subDir);
    expect(await io.directoryExists(subDir)).toBe(false);

    await io.save(file, content);
    expect(await io.read(file)).toBe(content);

    await io.deleteFileOrDirectory(file);
    expect(await io.fileExists(file)).toBe(false);

    await io.save(file, content);
    expect(await io.read(file)).toBe(content);

    const typePackage = await io.load(io.join(__dirname, '..', 'package.json'));
    expect(typePackage.name).toBe('@rimtrans/io');
  });

  afterAll(async () => {
    io.deleteFileOrDirectory(io.join(__dirname, '.tmp'));
  });
});
