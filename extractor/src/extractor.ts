import * as io from '@rimtrans/io';
import { PrettierOptions } from './xml';
import * as definition from './definition';
import * as typePackage from './type-package';
import * as injection from './injection';
import * as keyed from './keyed-replacement';
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
 * @param paths the array of paths to mod directories, order: `[core, ...mods]`.
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
  ] = await Promise.all([
    definition.load(mods.map(mod => mod.pathDefs)).then(definition.resolveInheritance),
    typePackage.load([...typePackages, ...mods.map(mod => mod.pathAssemblies)]),
    Promise.all(
      languages.map(lang => injection.load(mods.map(mod => mod.pathDefInjected(lang)))),
    ),
    keyed.load(mods.map(mod => mod.pathKeyed(DEFAULT_LANGUAGE))),
    Promise.all(languages.map(lang => keyed.load(mods.map(mod => mod.pathKeyed(lang))))),
  ]);

  const newInjectionMaps = injection.parse(definitionMaps, classInfoMap, fuzzy);

  await Promise.all(
    languages.map(async (lang, langIndex) => {
      const oldInjectionMaps = languagesToOldInjectionMaps[langIndex];
      const mergedInjectionMaps = mods.map((mod, modIndex) =>
        injection.merge(newInjectionMaps[modIndex], oldInjectionMaps[modIndex]),
      );
      injection.checkDuplicated(mergedInjectionMaps);

      const oldKeyedMaps = languagesToOldKeyedMaps[langIndex];
      const mergedKeyedMaps = mods.map((mod, modIndex) =>
        keyed.merge(englishKeyedMaps[modIndex], oldKeyedMaps[modIndex]),
      );
      keyed.checkDuplicated(mergedKeyedMaps);

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
            keyed.save(
              output.pathKeyed(lang),
              mergedKeyedMaps[modIndex],
              prettierOptions,
            ),
          ]);
        }),
      );
    }),
  );

  return mods;
}
