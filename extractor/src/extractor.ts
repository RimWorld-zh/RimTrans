import * as io from '@rimtrans/io';
import { PrettierOptions } from './xml';
import { Mod, ModOutput } from './mod';
import { TypePackage } from './type-package';
import { Definition } from './definition';
import { Injection } from './injection';
import { KeyedReplacement } from './keyed-replacement';
import { StringsFile } from './strings-file';
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

export class Extractor {
  /**
   *
   * @param paths the array of paths to mod directories, `[Core, ...Mods]`.
   */
  public static async extract(solution: ExtractorSolution): Promise<Mod[]> {
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
      Definition.load(mods.map(mod => mod.pathDefs)).then(Definition.resolveInheritance),

      // type
      TypePackage.load([...typePackages, ...mods.map(mod => mod.pathAssemblies)]),

      // DefInjected
      Promise.all(
        languages.map(lang => Injection.load(mods.map(mod => mod.pathDefInjected(lang)))),
      ),

      // Keyed
      KeyedReplacement.load(mods.map(mod => mod.pathKeyed(DEFAULT_LANGUAGE))),
      Promise.all(
        languages.map(lang =>
          KeyedReplacement.load(mods.map(mod => mod.pathKeyed(lang))),
        ),
      ),

      // Strings
      StringsFile.load(mods.map(mod => mod.pathStrings(DEFAULT_LANGUAGE))),
      Promise.all(
        languages.map(lang => StringsFile.load(mods.map(mod => mod.pathStrings(lang)))),
      ),
    ]);

    const newInjectionMaps = Injection.parse(definitionMaps, classInfoMap, fuzzy);

    await Promise.all(
      languages.map(async (lang, langIndex) => {
        // DefInjected
        const oldInjectionMaps = languagesToOldInjectionMaps[langIndex];
        const mergedInjectionMaps = mods.map((mod, modIndex) =>
          Injection.merge(newInjectionMaps[modIndex], oldInjectionMaps[modIndex]),
        );
        Injection.checkDuplicated(mergedInjectionMaps);

        // Keyed
        const oldKeyedMaps = languagesToOldKeyedMaps[langIndex];
        const mergedKeyedMaps = mods.map((mod, modIndex) =>
          KeyedReplacement.merge(englishKeyedMaps[modIndex], oldKeyedMaps[modIndex]),
        );
        KeyedReplacement.checkDuplicated(mergedKeyedMaps);

        // Strings
        const oldStringsMaps = languagesToOldStringsMaps[langIndex];
        const mergedStringsMaps = mods.map((mod, modIndex) =>
          StringsFile.merge(englishStringsMaps[modIndex], oldStringsMaps[modIndex]),
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
              Injection.save(
                output.pathDefInjected(lang),
                mergedInjectionMaps[modIndex],
                prettierOptions,
              ),
              KeyedReplacement.save(
                output.pathKeyed(lang),
                mergedKeyedMaps[modIndex],
                prettierOptions,
              ),
              StringsFile.save(
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
}
