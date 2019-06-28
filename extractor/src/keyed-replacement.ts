import * as io from '@rimtrans/io';
import {
  XNodeData,
  XTextData,
  XCommentData,
  XElementData,
  loadXML,
  PrettierOptions,
  resolveXmlPrettierOptions,
  saveXML,
} from './xml';
import {
  TAG_NAME_LANGUAGE_DATA,
  TEXT_EN,
  TEXT_TODO,
  TEXT_UNUSED,
  TEXT_NEWLINE,
} from './constants';

export interface KeyedReplacement {
  key: string;
  origin: string;
  translation: string;
  unused?: boolean;
  duplicated?: boolean;
}

export interface KeyedReplacementMap {
  [fileName: string]: (string | KeyedReplacement)[];
}

/**
 * Load `Keyed` xml document files of the mod and get `KeyedReplacementMap`.
 * @param keyedDirectories the path to the `Keyed` directory of the mod
 */
export async function load(keyedDirectories: string[]): Promise<KeyedReplacementMap[]> {
  return Promise.all(
    keyedDirectories.map(async dir => {
      const map: KeyedReplacementMap = {};
      if (!(await io.directoryExists(dir))) {
        return map;
      }

      await io
        .search(['**/*.xml'], { cwd: dir, case: false, onlyFiles: true })
        .then(files =>
          Promise.all(
            files.map(async fileName => {
              const root = await loadXML(io.join(dir, fileName));
              map[fileName] = [];
              const keyedList = map[fileName];

              let comment = '';
              let origin = '';
              root.childNodes.forEach(node => {
                switch (node.nodeType) {
                  case 'comment':
                    comment = node.value.trim();
                    if (comment === TEXT_UNUSED) {
                      // do nothing
                    } else if (comment.startsWith(TEXT_EN)) {
                      origin = comment.replace(TEXT_EN, '').trim();
                    } else {
                      keyedList.push(comment);
                    }
                    break;

                  case 'element':
                    keyedList.push({
                      key: node.name,
                      origin,
                      translation: node.value,
                    });
                    origin = '';
                    break;

                  default:
                    if (node.value.split(/\r|\n|\r\n/).length > 2) {
                      keyedList.push(TEXT_NEWLINE);
                    }
                }
              });
            }),
          ),
        );

      return map;
    }),
  );
}

/**
 * Merge old translation keyed map to the origin keyed map, return a new map
 * @param originMap the origin(English) keyed map
 * @param oldMap the old translation keyed map
 */
export function merge(
  originMap: KeyedReplacementMap,
  oldMap: KeyedReplacementMap,
): KeyedReplacementMap {
  const newMap: KeyedReplacementMap = {};

  Object.entries(originMap).forEach(([fileName, keyedList]) => {
    newMap[fileName] = keyedList.map<string | KeyedReplacement>(keyed => {
      if (typeof keyed === 'string') {
        return keyed;
      }
      return {
        key: keyed.key,
        origin: keyed.translation,
        translation: TEXT_TODO,
      };
    });
  });

  Object.entries(oldMap).forEach(([fileName, oldKeyedList]) => {
    const newKeyedList = newMap[fileName] || (newMap[fileName] = []);

    const unusedKeyeds = [] as KeyedReplacement[];
    const endComments = [] as string[];

    oldKeyedList.forEach(oldKeyed => {
      if (typeof oldKeyed === 'string') {
        if (oldKeyed === TEXT_NEWLINE) {
          return;
        }
        if (!newKeyedList.includes(oldKeyed)) {
          endComments.push(oldKeyed);
        }
        return;
      }
      const newKeyed = newKeyedList.find(
        (k): k is KeyedReplacement => typeof k === 'object' && k.key === oldKeyed.key,
      );
      if (!newKeyed) {
        unusedKeyeds.push(oldKeyed);
        return;
      }
      if (
        oldKeyed.translation &&
        oldKeyed.translation !== TEXT_TODO &&
        oldKeyed.translation !== newKeyed.origin
      ) {
        newKeyed.translation = oldKeyed.translation;
      }
    });

    if (unusedKeyeds.length > 0) {
      newKeyedList.push(
        TEXT_NEWLINE,
        ...unusedKeyeds.map(keyed => ({
          ...keyed,
          unused: true,
        })),
      );
    }

    if (endComments.length > 0) {
      newKeyedList.push(TEXT_NEWLINE, ...endComments);
    }
  });

  return newMap;
}

/**
 * Check duplicated keyed, should be call after `merge()`.
 * @param keyedReplacementMaps the array of `KeyedReplacementMap`, `[Core, ...Mods]`
 */
export function checkDuplicated(
  keyedReplacementMaps: KeyedReplacementMap[],
): KeyedReplacementMap[] {
  const visited = new Set<string>();
  keyedReplacementMaps.forEach(map =>
    Object.keys(map)
      .sort()
      .forEach(fileName => {
        const keyedList = map[fileName];
        keyedList.forEach(keyed => {
          if (typeof keyed === 'string') {
            return;
          }
          if (visited.has(keyed.key)) {
            keyed.duplicated = true;
            return;
          }
          visited.add(keyed.key);
        });
      }),
  );
  return keyedReplacementMaps;
}

/**
 * Save `KeyedReplacementMap` as XML document files.
 * @param directory the directory to save to
 * @param keyedReplaceMap the `KeyedReplacementMap`
 * @param prettierOptions format options
 */
export async function save(
  directory: string,
  keyedReplaceMap: KeyedReplacementMap,
  prettierOptions?: PrettierOptions,
): Promise<void> {
  const { tab, indent, eol, newline } = resolveXmlPrettierOptions(prettierOptions);

  const promises: Promise<void>[] = [];

  Object.entries(keyedReplaceMap).forEach(([fileName, keyedList]) => {
    const blocks: XNodeData[][] = [];
    let currentBlock: XNodeData[] = [];
    blocks.push(currentBlock);

    keyedList.forEach(keyed => {
      if (typeof keyed === 'string') {
        if (keyed === TEXT_NEWLINE) {
          currentBlock = [];
          blocks.push(currentBlock);
          return;
        }
        currentBlock.push(
          indent,
          {
            nodeType: 'comment',
            value: ` ${keyed} `,
          },
          newline,
        );
        return;
      }
      if (keyed.duplicated) {
        return;
      }
      if (keyed.unused) {
        currentBlock.push(
          indent,
          {
            nodeType: 'comment',
            value: ` ${TEXT_UNUSED} `,
          },
          newline,
        );
      }
      currentBlock.push(
        indent,
        { nodeType: 'comment', value: ` ${TEXT_EN} ${keyed.origin} ` },
        newline,
        indent,
        {
          nodeType: 'element',
          name: keyed.key,
          attributes: {},
          childNodes: [{ nodeType: 'text', value: keyed.translation }],
          elements: [],
          value: keyed.translation,
        },
        newline,
      );
    });

    const childNodes: XNodeData[] = [newline, newline];

    blocks.forEach(block => {
      if (block.length > 0) {
        childNodes.push(...block, newline);
      }
    });

    const languageData: XElementData = {
      nodeType: 'element',
      name: TAG_NAME_LANGUAGE_DATA,
      attributes: {},
      childNodes,
      elements: childNodes.filter((c): c is XElementData => c.nodeType === 'element'),
      value: '',
    };

    promises.push(
      saveXML(io.join(directory, fileName), languageData, false, prettierOptions),
    );
  });

  await Promise.all(promises);
}
