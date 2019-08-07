import { genPathResolve } from '@huiji/shared-utils';
import * as io from '@rimtrans/io';
import { pathTestMods, pathCore, outputMods } from './utils.test';
import { Mod } from './mod';

const resolvePath = genPathResolve(__dirname, '..', '..');

describe('mod', () => {
  let mods: Mod[];
  let modIds: string[];

  beforeAll(async () => {
    modIds = await io.search(['*'], {
      cwd: pathTestMods,
      onlyDirectories: true,
    });
  });

  test('error-about', async () => {
    const mod = await Mod.load(io.join(__dirname, '..', 'tests', 'ErrorMod'));
    expect(mod.meta.description.includes('Error:'));
  });

  test('test-mods', async () => {
    mods = await Promise.all(modIds.map(id => Mod.load(io.join(pathTestMods, id))));
  });

  test('mock', async () => {
    const pathMock = io.join(__dirname, 'Mock');
    const mock = await Mod.load(io.join(__dirname, 'Mock'));

    expect(mock.path).toBe(pathMock);
    expect(mock.id).toBe('Mock');
    expect(mock.workshopId).toBe(undefined);

    expect(mock.meta.path).toBe(pathMock);
    expect(mock.meta.id).toBe('Mock');
    expect(mock.meta.workshopId).toBe(undefined);
    expect(mock.meta.name).toBe('Mock');
    expect(mock.meta.author).toBe('Anonymous');
    expect(mock.meta.url).toBe('');
    expect(mock.meta.description).toBe('No description provided.');
    expect(mock.meta.targetVersion).toBe('Unknown');
    expect(mock.meta.supportedVersions).toEqual([]);

    mods.unshift(mock);
  });

  test('core', async () => {
    const core = await Mod.load(pathCore);

    expect(core.meta.workshopId).toBe(undefined);

    expect(core.path).toBe(pathCore);

    expect(core.previewImage).toBe(io.join(pathCore, 'About', 'Preview.png'));

    expect(core.pathAbout).toBe(io.join(pathCore, 'About'));
    expect(core.pathAssemblies).toBe(io.join(pathCore, 'Assemblies'));
    expect(core.pathDefs).toBe(io.join(pathCore, 'Defs'));
    expect(core.pathLanguages).toBe(io.join(pathCore, 'Languages'));
    expect(core.pathPatches).toBe(io.join(pathCore, 'Patches'));
    expect(core.pathTextures).toBe(io.join(pathCore, 'Textures'));

    expect(core.meta.id).toBe('Core');
    expect(core.meta.name).toBe('Core');
    expect(core.meta.author).toBe('Ludeon Studios');
    expect(core.meta.url).toBe('http://rimworldgame.com');
    expect(core.meta.description).toBe('The core game content for RimWorld.');
    expect(core.meta.targetVersion).toBe('Unknown');
    expect(core.meta.supportedVersions).toEqual([]);

    expect(core.pathLanguage('MockLanguage')).toBe(
      io.join(pathCore, 'Languages', 'MockLanguage'),
    );
    expect(core.pathBackstories('MockLanguage')).toBe(
      io.join(pathCore, 'Languages', 'MockLanguage', 'Backstories'),
    );
    expect(core.pathDefInjected('MockLanguage')).toBe(
      io.join(pathCore, 'Languages', 'MockLanguage', 'DefInjected'),
    );
    expect(core.pathKeyed('MockLanguage')).toBe(
      io.join(pathCore, 'Languages', 'MockLanguage', 'Keyed'),
    );
    expect(core.pathStrings('MockLanguage')).toBe(
      io.join(pathCore, 'Languages', 'MockLanguage', 'Strings'),
    );

    const dir = io.join(__dirname, 'Mock');
    const output = core.output(dir);
    expect(output.pathLanguage('MockLanguage')).toBe(
      io.join(dir, 'Languages', 'MockLanguage'),
    );
    expect(output.pathBackstories('MockLanguage')).toBe(
      io.join(dir, 'Languages', 'MockLanguage', 'Backstories'),
    );
    expect(output.pathDefInjected('MockLanguage')).toBe(
      io.join(dir, 'Languages', 'MockLanguage', 'DefInjected'),
    );
    expect(output.pathKeyed('MockLanguage')).toBe(
      io.join(dir, 'Languages', 'MockLanguage', 'Keyed'),
    );
    expect(output.pathStrings('MockLanguage')).toBe(
      io.join(dir, 'Languages', 'MockLanguage', 'Strings'),
    );

    mods.unshift(core);
  });

  afterAll(async () => {
    await io.save(outputMods, JSON.stringify(mods, undefined, '  '));
  });
});
