import { English } from './languages/english';

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
   * Translating progress of the language, compute by program.
   */
  progress?: number;
}

export type LanguageDictionary = {
  readonly [key in keyof (typeof English)['dict']]: string
};

export interface LanguageData {
  readonly info: LanguageInfo;
  readonly dict: Partial<LanguageDictionary>;
}
