import { LanguageInfo, LanguageData } from './models';
import { TranslationDictionary } from './models-dict';
import * as languages from './languages';
import * as progresses from './progresses';

export * from './models';
export * from './models-dict';

type LanguageID = keyof typeof languages;

const languageList: LanguageData[] = (Object.keys(languages) as LanguageID[])
  .sort()
  .map(id => {
    const lang: LanguageData = languages[id];
    lang.info.progress = progresses[id];
    return lang;
  });

export const languageInfos: LanguageInfo[] = languageList.map(lang => lang.info);

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

function clone<T>(obj: T): T {
  return JSON.parse(JSON.stringify(obj));
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function mergeDict<T extends any>(target: T, source: any): T {
  Object.keys(source).forEach(k => {
    const vt = target[k];
    const vs = source[k];
    if (vs) {
      if (typeof vs === 'string' && typeof vt === 'string') {
        target[k] = vs;
      } else if (typeof vs === 'object' && typeof vt === 'object') {
        mergeDict(vt, vs);
      }
    }
  });

  return target;
}

/**
 * Get dictionary for specified language ID.
 * @param languageID the ID of the language
 */
export function getLanguageDictByID(languageID: string): TranslationDictionary {
  const dict = clone(languages.English.dict) as TranslationDictionary;

  if (languageID === 'English') {
    return dict;
  }

  const translation = languageList.find(l => l.info.languageID === languageID);
  if (translation) {
    mergeDict(dict, translation.dict);
  }

  return dict;
}
