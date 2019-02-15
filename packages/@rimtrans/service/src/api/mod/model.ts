// tslint:disable:missing-jsdoc
// names
export const ABOUT = 'About';
export const ABOUT_XML = 'About.xml';
export const PREVIEW_PNG = 'Preview.png';
export const DEFS = 'Defs';
export const LANGUAGES = 'Languages';
export const LANGUAGE_INFO_XML = 'LanguageInfo.xml';
export const LANG_ICON_PNG = 'LangIcon.png';
export const FRIENDLY_NAME_TXT = 'FriendlyName.txt';
export const TIMESTAMP = 'timestamp';

export interface LanguageCollection {
  timestamp: number;
  items: LanguageData[];
}

export interface LanguageData {
  name: string;
  internal: boolean;
  status: 'success' | 'pending' | 'failed';
  /** current file size in downloading process */
  current?: number;
  /** total file size, maybe NaN */
  total?: number;
  info?: string;
  friendly?: string;
}

export type ModCategory = 'internal' | 'workshop' | 'custom';

export interface ModInfo {
  category: ModCategory;
  path: string;
  about: string;
}

export interface ModsParams {
  /** The path to the directory of mods, e.g. `/RimWorld/Mods` */
  path: string;
}
