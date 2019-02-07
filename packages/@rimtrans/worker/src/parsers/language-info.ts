import { parse } from '../parse';

export interface LanguageInfo {
  friendlyNameNative: string;
  friendlyNameEnglish: string;
}

/**
 * Parse LanguageInfo.xml
 */
export function languageInfo(xml: string): LanguageInfo {
  const doc = parse(xml);
  if (doc.firstElementChild) {
    const children = Array.from(doc.firstElementChild.children);
    const friendlyNameNative = children.find(c => c.tagName === 'friendlyNameNative');
    const friendlyNameEnglish = children.find(c => c.tagName === 'friendlyNameEnglish');

    return {
      friendlyNameNative:
        (friendlyNameNative && friendlyNameNative.innerHTML.trim()) || '',
      friendlyNameEnglish:
        (friendlyNameEnglish && friendlyNameEnglish.innerHTML.trim()) || '',
    };
  } else {
    throw new Error(`Missing root element in the XML document.\n${xml}\n`);
  }
}
