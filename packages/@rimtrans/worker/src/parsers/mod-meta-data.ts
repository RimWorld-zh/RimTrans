import { parse } from '../parse';

export interface ModMetaData {
  name: string;
  author: string;
  targetVersion: string;
  supportedVersions: string[];
  url: string;
  description: string;
}

/**
 * Parse About.xml
 */
export function modMetaData(xml: string): ModMetaData {
  const data: ModMetaData = {
    name: 'Unnamed',
    author: 'Anonymous',
    targetVersion: '',
    supportedVersions: [],
    url: '',
    description: '',
  };

  const doc = parse(xml);
  const root = doc.firstElementChild;

  if (!root) {
    throw Error('Miss root element in About.xml');
  }

  const children = Array.from(root.children);

  (Object.keys(data) as (keyof ModMetaData)[]).forEach(key => {
    const elem = children.find(c => c.tagName === key);
    if (key === 'supportedVersions') {
      const items: string[] =
        (elem &&
          Array.from(elem.children)
            .map(li => (li.textContent && li.textContent.trim()) || '')
            .filter(c => !!c)) ||
        [];
      data[key] = items;
    } else {
      const content = ((elem && elem.textContent) || '').trim();
      if (content) {
        data[key] = content;
      }
    }
  });

  return data;
}
