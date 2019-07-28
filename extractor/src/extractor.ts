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

  /**
   * Run in brand new mode or not, this will clean old language data.
   */
  brandNewMode?: boolean;

  /**
   * Debug mode will output language intermediate data (json format) before serialize to xml.
   */
  debugMode?: boolean;
}

export class Extractor {
  /**
   *
   * @param paths the array of paths to mod directories, `[Core, ...Mods]`.
   */
  public static async extract(config: ExtractorConfig): Promise<Mod[]> {
    const {
      temp,
      typePackages,
      fuzzy,
      prettierOptions,
      modConfigs,
      languages,
      brandNewMode,
      debugMode,
    } = config;

    const mods = await Promise.all(modConfigs.map(({ path }) => Mod.load(path)));
    const outputs = mods.map(
      (mod, modIndex): ModOutput => {
        const cfg = modConfigs[modIndex];
        if (cfg.outputAsMod && cfg.outputPath) {
          return mod.output(cfg.outputPath);
        }
        return mod;
      },
    );

    if (brandNewMode) {
      const toDeleteDirs: string[] = [];
      languages.forEach(lang =>
        outputs.forEach(output =>
          toDeleteDirs.push(
            output.pathDefInjected(lang),
            output.pathKeyed(lang),
            output.pathStrings(lang),
          ),
        ),
      );
      await Promise.all(
        toDeleteDirs.map(async dir => {
          if (await io.directoryExists(dir)) {
            await io.deleteFileOrDirectory(dir);
          }
        }),
      );
    } else {
      await Promise.all(
        modConfigs.map(async (cfg, modIndex) => {
          if (cfg.outputAsMod && cfg.outputPath) {
            const mod = mods[modIndex];
            const output = outputs[modIndex];
            await Promise.all(
              languages.map(async lang => {
                const srcLang = mod.pathLanguage(lang);
                const destLang = output.pathLanguage(lang);
                const [srcLangExists, destLangExists] = await Promise.all([
                  io.directoryExists(srcLang),
                  io.directoryExists(destLang),
                ]);
                if (srcLangExists && !destLangExists) {
                  const destLangParent = io.directoryName(destLang);
                  if (!(await io.directoryExists(destLangParent))) {
                    await io.createDirectory(destLangParent);
                  }
                  await io.copy(srcLang, destLang);
                }
              }),
            );
          }
        }),
      );
    }

    const [
      definitionMaps,
      classInfoMap,
      englishKeyedMaps,
      englishStringsMaps,
      languagesToOldInjectionMaps,
      languagesToOldKeyedMaps,
      languagesToOldStringsMaps,
    ] = await Promise.all([
      // Defs
      Definition.load(mods.map(mod => mod.pathDefs)).then(Definition.resolveInheritance),

      // Assemblies
      TypePackage.load([...typePackages, ...mods.map(mod => mod.pathAssemblies)]),

      // English Keyed and Strings
      KeyedReplacement.load(mods.map(mod => mod.pathKeyed(DEFAULT_LANGUAGE))),
      StringsFile.load(mods.map(mod => mod.pathStrings(DEFAULT_LANGUAGE))),
      // DefInjected
      Promise.all(
        languages.map(lang =>
          Injection.load(outputs.map(output => output.pathDefInjected(lang))),
        ),
      ),

      // Keyed
      Promise.all(
        languages.map(lang =>
          KeyedReplacement.load(outputs.map(output => output.pathKeyed(lang))),
        ),
      ),

      // Strings
      Promise.all(
        languages.map(lang =>
          StringsFile.load(outputs.map(output => output.pathStrings(lang))),
        ),
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
          modConfigs.map(async (cfg, modIndex) => {
            if (!cfg.extract) {
              return;
            }
            const output = outputs[modIndex];
            if (debugMode) {
              await io.save(
                io.join(output.pathLanguage(lang), 'intermediate-data.json'),
                JSON.stringify(
                  {
                    DefInjected: mergedInjectionMaps[modIndex],
                    Keyed: mergedKeyedMaps[modIndex],
                    Strings: mergedStringsMaps[modIndex],
                  },
                  undefined,
                  '  ',
                ),
              );
            }
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
