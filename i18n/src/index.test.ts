import * as languages from './languages';
import { languageInfos, getLanguageIdByCode, getLanguageDictByID } from './index';

describe('i18n', () => {
  test('language infos', () => {
    languageInfos.forEach(a => {
      languageInfos.forEach(b => {
        if (a !== b) {
          expect(a.languageCodes).not.toEqual(b.languageCodes);
        }
      });
    });

    expect(Object.keys(languages).sort()).toEqual(
      languageInfos.map(info => info.languageID),
    );
  });

  test('get id', () => {
    expect(getLanguageIdByCode('en-US')).toBe('English');
    expect(getLanguageIdByCode('en')).toBe('English');
    expect(getLanguageIdByCode('00')).toBe('English');

    expect(getLanguageIdByCode('zh-CN')).toBe('ChineseSimplified');
    expect(getLanguageIdByCode('zh')).toBe('ChineseSimplified');
    expect(getLanguageIdByCode('zh-XX')).toBe('ChineseSimplified');

    expect(getLanguageIdByCode('zh-TW')).toBe('ChineseTraditional');
  });

  test('get dict', () => {
    const english = getLanguageDictByID('English');

    const mocking = getLanguageDictByID('Mocking');
    expect(mocking).toEqual(english);

    const chinese = getLanguageDictByID('ChineseSimplified');
    expect(chinese).not.toEqual(english);
    expect(english.app.name).toBe('RimTrans');
    expect(chinese.app.name).toBe('边缘译');
    expect(english.app.name).not.toBe(chinese.app.name);
  });
});
