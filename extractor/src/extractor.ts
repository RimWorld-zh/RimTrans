import * as io from '@rimtrans/io';
import { DEFAULT_LANGUAGE } from './constants';
import { ExtractorEventEmitter } from './extractor-event-emitter';
import { PrettierOptions } from './xml';
import { Mod, ModOutput } from './mod';
import { TypePackageExtractor } from './type-package';
import { DefinitionExtractor } from './definition';
import { Injection, InjectionExtractor } from './injection';
import { KeyedReplacementExtractor } from './keyed-replacement';
import { StringsFileExtractor } from './strings-file';

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
  public readonly emitter: ExtractorEventEmitter;

  /* eslint-disable lines-between-class-members */
  private readonly typePackageExtractor: TypePackageExtractor;
  private readonly definitionExtractor: DefinitionExtractor;
  private readonly injectionExtractor: InjectionExtractor;
  private readonly keyedReplacementExtractor: KeyedReplacementExtractor;
  private readonly stringsFileExtractor: StringsFileExtractor;
  /* eslint-enable lines-between-class-members */

  public constructor() {
    const emitter = new ExtractorEventEmitter();
    this.emitter = emitter;
    this.typePackageExtractor = new TypePackageExtractor(emitter);
    this.definitionExtractor = new DefinitionExtractor(emitter);
    this.injectionExtractor = new InjectionExtractor(emitter);
    this.keyedReplacementExtractor = new KeyedReplacementExtractor(emitter);
    this.stringsFileExtractor = new StringsFileExtractor(emitter);
  }

  /**
   *
   * @param paths the array of paths to mod directories, `[Core, ...Mods]`.
   */
  public async extract(config: ExtractorConfig): Promise<Mod[]> {
    const start = Date.now();

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
      await Promise.all(toDeleteDirs.map(async dir => io.deleteFileOrDirectory(dir)));
    } else {
      const toCopyDirs: [string, string][] = [];
      languages.forEach(lang =>
        modConfigs.forEach((cfg, modIndex) => {
          if (cfg.outputAsMod && cfg.outputPath) {
            const mod = mods[modIndex];
            const output = outputs[modIndex];
            toCopyDirs.push(
              [mod.pathDefInjected(lang), output.pathDefInjected(lang)],
              [mod.pathKeyed(lang), output.pathKeyed(lang)],
              [mod.pathStrings(lang), output.pathStrings(lang)],
            );
          }
        }),
      );

      await Promise.all(
        toCopyDirs.map(async ([src, dest]) => {
          const [srcExists, destExists] = await Promise.all([
            io.directoryExists(src),
            io.directoryExists(dest),
          ]);
          if (srcExists && !destExists) {
            await io.copy(src, dest);
          }
        }),
      );
    }

    const [
      definitionMaps,
      { classInfoMap },
      englishKeyedMaps,
      englishStringsMaps,
      languagesToOldInjectionMaps,
      languagesToOldKeyedMaps,
      languagesToOldStringsMaps,
    ] = await Promise.all([
      // Defs
      this.definitionExtractor
        .load(mods.map(mod => mod.pathDefs))
        .then(maps => this.definitionExtractor.resolveInheritance(maps)),

      // Assemblies
      this.typePackageExtractor.load([
        ...typePackages,
        ...mods.map(mod => mod.pathAssemblies),
      ]),

      // English Keyed
      this.keyedReplacementExtractor.load(
        mods.map(mod => mod.pathKeyed(DEFAULT_LANGUAGE)),
      ),

      // English Strings
      this.stringsFileExtractor.load(mods.map(mod => mod.pathStrings(DEFAULT_LANGUAGE))),

      // DefInjected
      Promise.all(
        languages.map(lang =>
          this.injectionExtractor.load(
            outputs.map(output => output.pathDefInjected(lang)),
          ),
        ),
      ),

      // Keyed
      Promise.all(
        languages.map(lang =>
          this.keyedReplacementExtractor.load(
            outputs.map(output => output.pathKeyed(lang)),
          ),
        ),
      ),

      // Strings
      Promise.all(
        languages.map(lang =>
          this.stringsFileExtractor.load(outputs.map(output => output.pathStrings(lang))),
        ),
      ),
    ]);

    const newInjectionMaps = this.injectionExtractor.parse(
      definitionMaps,
      classInfoMap,
      fuzzy,
    );

    await Promise.all(
      languages.map(async (lang, langIndex) => {
        // DefInjected
        const oldInjectionMaps = languagesToOldInjectionMaps[langIndex];
        const mergedInjectionMaps = mods.map((mod, modIndex) =>
          this.injectionExtractor.merge(
            newInjectionMaps[modIndex],
            oldInjectionMaps[modIndex],
          ),
        );
        this.injectionExtractor.checkDuplicated(mergedInjectionMaps);

        // Keyed
        const oldKeyedMaps = languagesToOldKeyedMaps[langIndex];
        const mergedKeyedMaps = await Promise.all(
          mods.map((mod, modIndex) =>
            this.keyedReplacementExtractor.merge(
              englishKeyedMaps[modIndex],
              oldKeyedMaps[modIndex],
            ),
          ),
        ).then(maps => this.keyedReplacementExtractor.checkDuplicated(maps));

        // Strings
        const oldStringsMaps = languagesToOldStringsMaps[langIndex];
        const mergedStringsMaps = mods.map((mod, modIndex) =>
          this.stringsFileExtractor.merge(
            englishStringsMaps[modIndex],
            oldStringsMaps[modIndex],
          ),
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
              this.injectionExtractor.save(
                output.pathDefInjected(lang),
                mergedInjectionMaps[modIndex],
                prettierOptions,
              ),
              this.keyedReplacementExtractor.save(
                output.pathKeyed(lang),
                mergedKeyedMaps[modIndex],
                prettierOptions,
              ),
              this.stringsFileExtractor.save(
                output.pathStrings(lang),
                mergedStringsMaps[modIndex],
                prettierOptions,
              ),
            ]);
          }),
        );
      }),
    );

    this.emitter.emit('done', `Extracting completed: ${Date.now() - start}ms`);

    return mods;
  }
}
