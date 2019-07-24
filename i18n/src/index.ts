import { LanguageInfo, LanguageDictionary, LanguageData } from './models';
import * as languages from './languages';

export * from './models';

type LanguageID = keyof typeof languages;

const languageList: LanguageData[] = (Object.keys(languages) as LanguageID[])
  .sort()
  .map(id => languages[id]);

const englishEntriesLength = Object.keys(languages.English.dict).length;

export const languageInfos: LanguageInfo[] = languageList.map(lang => {
  lang.info.progress = Object.keys(lang.dict).length / englishEntriesLength;
  return lang.info;
});

export function getLanguageIdByCode(languageCode: string): string {
  let code = languageCode;
  let info = languageInfos.find(i => i.languageCodes.includes(code));
  if (!info && code.includes('-')) {
    code = code.replace(/-.+/, '');
    info = languageInfos.find(i => i.languageCodes.includes(code));
  }
  if (info) {
    return info.languageID;
  }

  return 'English';
}

/**
 * Get dictionary for specified language ID.
 * @param languageID the ID of the language
 */
export function getLanguageDictByID(languageID: string): LanguageDictionary {
  if (languageID === 'English') {
    return { ...languages.English.dict };
  }

  const translation = languageList.find(l => l.info.languageID === languageID);
  if (translation) {
    return {
      ...languages.English.dict,
      ...translation.dict,
    };
  }

  return { ...languages.English.dict };
}
