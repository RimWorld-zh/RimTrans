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

export interface Languages {
  timestamp: number;
  items: LanguageData[];
}

export interface LanguageData {
  name: string;
  info?: string;
  friendly?: string;
}
