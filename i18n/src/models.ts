import { TranslationDictionaryPartial } from './models-dict';

export interface LanguageInfo {
  readonly translators: string[];

  /**
   * The language ID for RimWorld. The name of folders in `RimWorld/Mods/Core/Languages`
   */
  readonly languageID: string;

  /**
   * The language locale codes,
   */
  readonly languageCodes: readonly string[];

  readonly languageNameNative: string;
  readonly languageNameEnglish: string;

  /**
   * Translation progress, count by `script/gen-progress.ts`.
   */
  progress?: number;
}

export interface LanguageData {
  readonly info: LanguageInfo;
  readonly dict: TranslationDictionaryPartial;
}
