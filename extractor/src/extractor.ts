import * as io from '@rimtrans/io';
import { PrettierOptions } from './xml';
import * as definition from './definition';
import * as typePackage from './type-package';
import * as injection from './injection';
import * as keyedReplacement from './keyed-replacement';
import * as stringsFile from './strings-file';
import { Mod, ModOutput } from './mod';
import { DEFAULT_LANGUAGE } from './constants';

export interface ExtractorSolution {
  typePackages: string[];
  modPaths: string[];
  enabledMods: boolean[];
  languages: string[];
  outputDirectory?: string;
  fuzzy?: boolean;
  prettierOptions?: PrettierOptions;
}

/**
 *
 * @param paths the array of paths to mod directories, `[Core, ...Mods]`.
 */
export async function extract(solution: ExtractorSolution): Promise<Mod[]> {
  const {
    typePackages,
    modPaths,
    enabledMods,
    languages,
    outputDirectory,
    fuzzy,
    prettierOptions,
  } = solution;

  const mods = await Promise.all(modPaths.map(path => Mod.load(path)));

  const [
    definitionMaps,
    classInfoMap,
    languagesToOldInjectionMaps,
    englishKeyedMaps,
    languagesToOldKeyedMaps,
    englishStringsMaps,
    languagesToOldStringsMaps,
  ] = await Promise.all([
    // Defs
    definition.load(mods.map(mod => mod.pathDefs)).then(definition.resolveInheritance),

    // type
    typePackage.load([...typePackages, ...mods.map(mod => mod.pathAssemblies)]),

    // DefInjected
    Promise.all(
      languages.map(lang => injection.load(mods.map(mod => mod.pathDefInjected(lang)))),
    ),

    // Keyed
    keyedReplacement.load(mods.map(mod => mod.pathKeyed(DEFAULT_LANGUAGE))),
    Promise.all(
      languages.map(lang => keyedReplacement.load(mods.map(mod => mod.pathKeyed(lang)))),
    ),

    // Strings
    stringsFile.load(mods.map(mod => mod.pathStrings(DEFAULT_LANGUAGE))),
    Promise.all(
      languages.map(lang => stringsFile.load(mods.map(mod => mod.pathStrings(lang)))),
    ),
  ]);

  const newInjectionMaps = injection.parse(definitionMaps, classInfoMap, fuzzy);

  await Promise.all(
    languages.map(async (lang, langIndex) => {
      // DefInjected
      const oldInjectionMaps = languagesToOldInjectionMaps[langIndex];
      const mergedInjectionMaps = mods.map((mod, modIndex) =>
        injection.merge(newInjectionMaps[modIndex], oldInjectionMaps[modIndex]),
      );
      injection.checkDuplicated(mergedInjectionMaps);

      // Keyed
      const oldKeyedMaps = languagesToOldKeyedMaps[langIndex];
      const mergedKeyedMaps = mods.map((mod, modIndex) =>
        keyedReplacement.merge(englishKeyedMaps[modIndex], oldKeyedMaps[modIndex]),
      );
      keyedReplacement.checkDuplicated(mergedKeyedMaps);

      // Strings
      const oldStringsMaps = languagesToOldStringsMaps[langIndex];
      const mergedStringsMaps = mods.map((mod, modIndex) =>
        stringsFile.merge(englishStringsMaps[modIndex], oldStringsMaps[modIndex]),
      );

      await Promise.all(
        mods.map(async (mod, modIndex) => {
          if (!enabledMods[modIndex]) {
            return;
          }
          const output: ModOutput =
            (outputDirectory && mod.output(io.join(outputDirectory, mod.identify))) ||
            mod;
          await Promise.all([
            injection.save(
              output.pathDefInjected(lang),
              mergedInjectionMaps[modIndex],
              prettierOptions,
            ),
            keyedReplacement.save(
              output.pathKeyed(lang),
              mergedKeyedMaps[modIndex],
              prettierOptions,
            ),
            stringsFile.save(
              output.pathStrings(lang),
              mergedStringsMaps[modIndex],
              prettierOptions,
            ),
          ]);
        }),
      );
    }),
  );

  return mods;
}
