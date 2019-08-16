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
    expect(await io.pathExists(io.join(__dirname, 'Mock', 'mock.txt'))).toBe(false);
    expect(await io.pathExists(__filename)).toBe(true);
    expect(await io.pathExists(`${__filename}.mock`)).toBe(false);
    expect(await io.pathExists(io.join(__dirname, 'mock', 'mock', 'mock.txt'))).toBe(
      false,
    );

    expect(await io.pathExists(__dirname)).toBe(true);
    expect(await io.pathExists(io.join(__dirname, 'mock'))).toBe(false);
    expect(await io.pathExists(io.join(__dirname, 'mock', 'mock'))).toBe(false);
  });

  test('copy', async () => {
    const dir = io.join(__dirname, '.tmp', 'test');
    await io.remove(dir);

    let error: Error | undefined;
    try {
      await io.copy(io.join(__dirname, 'foobar', 'io.ts'), io.join(dir, 'io.ts'));
    } catch (e) {
      error = e;
    }
    expect(error).toBeTruthy();

    await io.copy(io.join(__dirname, 'io.ts'), io.join(dir, 'io.ts'));
    expect(await io.pathExists(io.join(dir, 'io.ts'))).toBe(true);

    await io.copy(io.join(__dirname, 'io.ts'), io.join(dir, 'foo', 'io.ts'));
    expect(await io.pathExists(io.join(dir, 'foo', 'io.ts'))).toBe(true);

    await io.copy(io.join(dir, 'foo'), io.join(dir, 'bar'));
    expect(await io.pathExists(io.join(dir, 'bar', 'io.ts'))).toBe(true);

    await io.remove(dir);
  });

  test('several', async () => {
    const subDir = io.join(__dirname, '.tmp', 'mock');
    const file = io.join(__dirname, '.tmp', 'mock', 'mock.txt');
    const content = 'mocking bird';

    await io.ensureDir(subDir);
    expect(await io.pathExists(subDir)).toBe(true);

    await io.remove(subDir);
    expect(await io.pathExists(subDir)).toBe(false);

    await io.outputFile(file, content);
    expect(await io.readFile(file, 'utf-8')).toBe(content);

    await io.remove(file);
    expect(await io.pathExists(file)).toBe(false);

    await io.outputFile(file, content);
    expect(await io.readFile(file, 'utf-8')).toBe(content);

    const typePackage = await io.readJson(io.join(__dirname, '..', 'package.json'));
    expect(typePackage.name).toBe('@rimtrans/io');
  });

  afterAll(async () => {
    io.remove(io.join(__dirname, '.tmp'));
  });
});
