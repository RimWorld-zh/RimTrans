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

  test('test-mods', async () => {
    mods = await Promise.all(modIds.map(id => Mod.load(io.join(pathTestMods, id))));
  });

  test('mock', async () => {
    const pathMock = io.join(__dirname, 'Mock');
    const mock = await Mod.load(io.join(__dirname, 'Mock'));

    expect(mock.identify).toBe('Mock');
    expect(mock.pathRoot).toBe(pathMock);

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

    expect(core.steamPublishFileId).toBe(undefined);

    expect(core.identify).toBe('Core');
    expect(core.pathRoot).toBe(pathCore);

    expect(core.previewImage).toBe(io.join(pathCore, 'About', 'Preview.png'));

    expect(core.pathAbout).toBe(io.join(pathCore, 'About'));
    expect(core.pathAssemblies).toBe(io.join(pathCore, 'Assemblies'));
    expect(core.pathDefs).toBe(io.join(pathCore, 'Defs'));
    expect(core.pathLanguages).toBe(io.join(pathCore, 'Languages'));
    expect(core.pathPatches).toBe(io.join(pathCore, 'Patches'));
    expect(core.pathTextures).toBe(io.join(pathCore, 'Textures'));

    expect(core.meta.name).toBe('Core');
    expect(core.meta.author).toBe('Ludeon Studios');
    expect(core.meta.url).toBe('http://rimworldgame.com');
    expect(core.meta.description).toBe('The core game content for RimWorld.');
    expect(core.meta.targetVersion).toBe('Unknown');
    expect(core.meta.supportedVersions).toEqual([]);

    expect(core.pathBackstories('English')).toBe(
      io.join(pathCore, 'Languages', 'English', 'Backstories'),
    );
    expect(core.pathDefInjected('English')).toBe(
      io.join(pathCore, 'Languages', 'English', 'DefInjected'),
    );
    expect(core.pathKeyed('English')).toBe(
      io.join(pathCore, 'Languages', 'English', 'Keyed'),
    );
    expect(core.pathStrings('English')).toBe(
      io.join(pathCore, 'Languages', 'English', 'Strings'),
    );

    mods.unshift(core);
  });

  afterAll(async () => {
    await io.save(outputMods, JSON.stringify(mods, undefined, '  '));
  });
});
