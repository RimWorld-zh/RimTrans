import pth from 'path';
import fse from 'fs-extra';
import globby from 'globby';

import { pathTestMods, pathCore, outputMods } from './utils.test';

import { Mod } from './mod';

describe('mod', () => {
  let mods: Mod[];
  let modIds: string[];

  beforeAll(async () => {
    modIds = await globby(['*'], {
      cwd: pathTestMods,
      onlyDirectories: true,
    });
  });

  test('error-about', async () => {
    const mod = await Mod.load(pth.join(__dirname, '..', 'tests', 'ErrorMod'));
    expect(mod.meta.description.includes('Error:'));
  });

  test('test-mods', async () => {
    mods = await Promise.all(modIds.map(id => Mod.load(pth.join(pathTestMods, id))));
  });

  test('mock', async () => {
    const pathMock = pth.join(__dirname, 'Mock');
    const mock = await Mod.load(pth.join(__dirname, 'Mock'));

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

    expect(core.previewImage).toBe(pth.join(pathCore, 'About', 'Preview.png'));

    expect(core.pathAbout).toBe(pth.join(pathCore, 'About'));
    expect(core.pathAssemblies).toBe(pth.join(pathCore, 'Assemblies'));
    expect(core.pathDefs).toBe(pth.join(pathCore, 'Defs'));
    expect(core.pathLanguages).toBe(pth.join(pathCore, 'Languages'));
    expect(core.pathPatches).toBe(pth.join(pathCore, 'Patches'));
    expect(core.pathTextures).toBe(pth.join(pathCore, 'Textures'));

    expect(core.meta.id).toBe('Core');
    expect(core.meta.name).toBe('Core');
    expect(core.meta.author).toBe('Ludeon Studios');
    expect(core.meta.url).toBe('http://rimworldgame.com');
    expect(core.meta.description).toBe('The core game content for RimWorld.');
    expect(core.meta.targetVersion).toBe('Unknown');
    expect(core.meta.supportedVersions).toEqual([]);

    expect(core.pathLanguage('MockLanguage')).toBe(
      pth.join(pathCore, 'Languages', 'MockLanguage'),
    );
    expect(core.pathBackstories('MockLanguage')).toBe(
      pth.join(pathCore, 'Languages', 'MockLanguage', 'Backstories'),
    );
    expect(core.pathDefInjected('MockLanguage')).toBe(
      pth.join(pathCore, 'Languages', 'MockLanguage', 'DefInjected'),
    );
    expect(core.pathKeyed('MockLanguage')).toBe(
      pth.join(pathCore, 'Languages', 'MockLanguage', 'Keyed'),
    );
    expect(core.pathStrings('MockLanguage')).toBe(
      pth.join(pathCore, 'Languages', 'MockLanguage', 'Strings'),
    );

    const dir = pth.join(__dirname, 'Mock');
    const output = core.output(dir);
    expect(output.pathLanguage('MockLanguage')).toBe(
      pth.join(dir, 'Languages', 'MockLanguage'),
    );
    expect(output.pathBackstories('MockLanguage')).toBe(
      pth.join(dir, 'Languages', 'MockLanguage', 'Backstories'),
    );
    expect(output.pathDefInjected('MockLanguage')).toBe(
      pth.join(dir, 'Languages', 'MockLanguage', 'DefInjected'),
    );
    expect(output.pathKeyed('MockLanguage')).toBe(
      pth.join(dir, 'Languages', 'MockLanguage', 'Keyed'),
    );
    expect(output.pathStrings('MockLanguage')).toBe(
      pth.join(dir, 'Languages', 'MockLanguage', 'Strings'),
    );

    mods.unshift(core);
  });

  afterAll(async () => {
    await fse.outputJSON(outputMods, mods, { spaces: 2 });
  });
});
