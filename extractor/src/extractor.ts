import * as io from '@rimtrans/io';
import { PrettierOptions } from './xml';
import { Mod, ModOutput } from './mod';
import { TypePackage } from './type-package';
import { Definition } from './definition';
import { Injection } from './injection';
import { KeyedReplacement } from './keyed-replacement';
import { StringsFile } from './strings-file';
import { DEFAULT_LANGUAGE } from './constants';

export interface ExtractorModConfig {
  /**
   * Path to the mod.
   */
  path: string;

  /**
   * Extract languages or not
   */
  extract: boolean;

  /**
   * Output languages as a stand alone mod or not.
   */
  outputAsMod?: boolean;

  /**
   * Output path.
   */
  outputPath?: string;
}

export interface ExtractorConfig {
  /**
   * Temporary directory
   */
  temp: string;

  /**
   * Paths to type package json files.
   */
  typePackages: string[];

  /**
   * Find possible translatable fields in fuzzy mode or not.
   */
  fuzzy?: boolean;

  /**
   * File format options
   */
  prettierOptions?: PrettierOptions;

  /**
   * Mods configs
   */
  modConfigs: ExtractorModConfig[];

  /**
   * Languages to extract.
   */
  languages: string[];
}

export class Extractor {
  /**
   *
   * @param paths the array of paths to mod directories, `[Core, ...Mods]`.
   */
  public static async extract(config: ExtractorConfig): Promise<Mod[]> {
    const { temp, typePackages, fuzzy, prettierOptions, modConfigs, languages } = config;

    const mods = await Promise.all(modConfigs.map(({ path }) => Mod.load(path)));

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
            const modConfig = modConfigs[modIndex];
            if (!modConfig.extract) {
              return;
            }
            const output: ModOutput =
              modConfig.outputAsMod && modConfig.outputPath
                ? mod.output(modConfig.outputPath)
                : mod;
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
