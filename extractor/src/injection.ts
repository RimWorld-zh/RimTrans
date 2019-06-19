import * as io from '@rimtrans/io';
import * as xml from './xml';
import { DefDocumentMap } from './definition';
import {
  ClassInfo,
  HandleInfo,
  EnumInfo,
  FieldInfo,
  TypeInfo,
  ATTRIBUTE_UNSAVED,
  ATTRIBUTE_NO_TRANSLATE,
  ATTRIBUTE_MUST_TRANSLATE,
  ATTRIBUTE_MAY_TRANSLATE,
  ATTRIBUTE_LOAD_ALIAS,
  ATTRIBUTE_TRANSLATION_CAN_CHANGE_COUNT,
  TYPE_STRING,
} from './type-package';

xml.mountDomPrototype();

// CONSTANTS
const FIELD_NAME_DEF_NAME = 'defName';
const TAG_NAME_LI = 'li';
const ATTRIBUTE_NAME_CLASS = 'Class';
const TAG_NAME_LANGUAGE_DATA = 'LanguageData';
const TEXT_EN = 'EN:';
const TEXT_TODO = 'TODO';
const TEXT_UNUSED = 'UNUSED';
const TEXT_FUZZY = 'FUZZY';

/**
 * The type of path nodes, if is tuple `[number, string]`, means `[index, handle]`
 */
export type PathNode = string | number | [number, string];

export interface Injection {
  path: PathNode[];
  origin: string | string[];
  translation: string | string[];
  unused?: boolean;
  duplicated?: boolean;
  fuzzy?: boolean;
}

export interface InjectionMap {
  [defType: string]: {
    [fileName: string]: (Injection | string)[];
  };
}

// ----------------------------------------------------------------
// Loading

/**
 * Load `DefInjected` xml documents and get `InjectionMap`.
 * @param paths the array of paths to `DefInjected` directories.
 */
export async function load(paths: string[]): Promise<any> {
  return Promise.all(
    paths.map(
      async (dir): Promise<InjectionMap> => {
        const map: InjectionMap = {};
        if (!(await io.directoryExists(dir))) {
          return map;
        }

        await io
          .search(['*'], {
            cwd: dir,
            onlyDirectories: true,
          })
          .then(defTypes =>
            Promise.all(
              defTypes.map(async defType => {
                map[defType] = {};
                const subMap = map[defType];

                const files = await io.search(['*.xml'], {
                  cwd: io.join(dir, defType),
                  case: false,
                  onlyFiles: true,
                });
                await Promise.all(
                  files.map(async xmlFile => {
                    const filePath = io.join(dir, defType, xmlFile);
                    const fileName = io.fileName(filePath, true);
                    subMap[fileName] = [];
                    const injections = subMap[fileName];
                    const doc = await xml.load(filePath);

                    let comment = '';
                    let origin: string | string[] = '';
                    let translation: string | string[] = TEXT_TODO;
                    Array.from(doc.documentElement.childNodes).forEach(node => {
                      switch (node.nodeType) {
                        case node.COMMENT_NODE:
                          comment = (node.nodeValue || '').trim();
                          if (comment === TEXT_UNUSED) {
                            // do nothing
                          }
                          if (comment.startsWith(TEXT_EN)) {
                            origin = comment.replace(TEXT_EN, '').trim();
                          } else {
                            injections.push(comment);
                          }
                          break;

                        case node.ELEMENT_NODE:
                          translation = (node as Element)
                            .getElements()
                            .map(li => li.elementValue.trim());
                          if (translation.length === 0) {
                            translation = TEXT_TODO;
                          }
                          injections.push({
                            path: (node as Element).tagName
                              .split('.')
                              .map(n => (/^\d+$/.test(n) ? Number.parseInt(n, 10) : n)),
                            origin,
                            translation,
                          });
                          break;

                        default:
                      }
                    });
                  }),
                );
              }),
            ),
          );

        return map;
      },
    ),
  );
}

// ----------------------------------------------------------------
// Parsing

/**
 * Parse the Def maps and get Def injection maps.
 * @param defMaps the array of Def maps
 * @param typeInfoMaps the array of DefInjection Map
 */
export function parse(
  defMaps: DefDocumentMap[],
  classInfoMap: Record<string, ClassInfo>,
): InjectionMap[] {
  const getPathNodes = generateGetPathNodes(classInfoMap);
  const parseField = generateParseField(classInfoMap, getPathNodes);
  const parseDef = generateParseDef(classInfoMap, parseField);

  const result: InjectionMap[] = defMaps.map(defMap => {
    const injectionMap: InjectionMap = {};

    Object.entries(defMap).forEach(([path, defDoc]) => {
      const filename = io.fileName(path);
      defDoc.documentElement.getElements().forEach(def => {
        const defType = def.tagName;
        const injectionFileMap = injectionMap[defType] || (injectionMap[defType] = {});
        const injections =
          injectionFileMap[filename] || (injectionFileMap[filename] = []);

        parseDef(injections as Injection[], def);
      });
    });

    return injectionMap;
  });

  return result;
}

// ----------------------------------------------------------------
// Parsing functions

export type ParseDef = (injections: Injection[], def: Element) => void;

export type ParseField = (
  path: PathNode[],
  injections: Injection[],
  typeInfo: TypeInfo,
  fieldInfo?: FieldInfo,
  element?: Element,
  pathNode?: PathNode,
) => void;

export type GetParseNodes = (list: Element[], typeInfoOf: TypeInfo) => PathNode[];

function tryGetElement(parent: Element, fieldInfo: FieldInfo): Element | undefined {
  let element: Element | undefined;
  if (fieldInfo.name) {
    element = parent.getElement(fieldInfo.name);
  }
  if (!element && fieldInfo.alias) {
    element = parent.getElement(fieldInfo.alias);
  }
  return element;
}

/**
 * High-order function: generate a function for parsing a Def element and getting `Injection`.
 * @param classInfoMap the `ClassInfo` map
 */
export function generateParseDef(
  classInfoMap: Record<string, ClassInfo>,
  parseField: ParseField,
): ParseDef {
  return (injections, def) => {
    const defNameElement = def.getElement(FIELD_NAME_DEF_NAME);
    const defName =
      defNameElement && defNameElement.elementValue && defNameElement.elementValue.trim();
    const classInfo: ClassInfo | undefined =
      classInfoMap[def.tagName] || classInfoMap.Def;

    if (!defName) {
      return;
    }

    classInfo.fields.forEach(fieldInfo => {
      if (fieldInfo.name === FIELD_NAME_DEF_NAME) {
        return;
      }
      parseField(
        [defName],
        injections,
        fieldInfo.type,
        fieldInfo,
        tryGetElement(def, fieldInfo),
      );
    });
  };
}

export function normalizeHandle(text: string): string {
  /* eslint-disable no-useless-escape */
  return text
    .trim()
    .replace(/[\ \n\t]+/g, '_')
    .replace(/[\r\.\-]+/g, '')
    .replace(/{.*?}/g, '')
    .replace(/[^0-9A-Za-z\-\_]+/g, '')
    .replace(/_+/g, '_')
    .replace(/^_+|_+$/g, '');
  /* eslint-enable no-useless-escape */
}

/**
 * High-order function: generate a function for parsing a field element and getting `Injection`
 * @param classInfoMap the `ClassInfo` map
 */
function generateParseField(
  classInfoMap: Record<string, ClassInfo>,
  getPathNodes: GetParseNodes,
): ParseField {
  /**
   * @param path
   * @param injections
   * @param typeInfo
   * @param fieldInfo if is undefined, means this is a list item
   * @param element
   * @param pathNode
   */
  const parseField: ParseField = (
    path,
    injections,
    typeInfo,
    fieldInfo,
    element,
    pathNode = -1,
  ) => {
    const { of: typeInfoOf } = typeInfo;

    const noTranslate =
      !!fieldInfo &&
      (fieldInfo.attributes.includes(ATTRIBUTE_UNSAVED) ||
        fieldInfo.attributes.includes(ATTRIBUTE_NO_TRANSLATE));
    const mustTranslate =
      !!fieldInfo &&
      (fieldInfo.attributes.includes(ATTRIBUTE_MUST_TRANSLATE) ||
        fieldInfo.attributes.includes(ATTRIBUTE_MAY_TRANSLATE));

    // skip
    if (noTranslate) {
      return;
    }

    // List<string>
    if (
      fieldInfo &&
      mustTranslate &&
      fieldInfo.attributes.includes(ATTRIBUTE_TRANSLATION_CAN_CHANGE_COUNT) &&
      typeInfo.category === 'LIST' &&
      typeInfoOf &&
      typeInfoOf.category === 'VALUE' &&
      typeInfoOf.name === TYPE_STRING &&
      element
    ) {
      const currentPath = [...path, fieldInfo.name];
      const origin: string[] = [];
      const translation: string[] = [];
      element.getElements().forEach(li => {
        origin.push(li.elementValue.trim());
        translation.push(TEXT_TODO);
      });
      injections.push({
        path: currentPath,
        origin,
        translation,
      });
      return;
    }

    // String
    if (
      fieldInfo &&
      mustTranslate &&
      typeInfo.category === 'VALUE' &&
      typeInfo.name === TYPE_STRING
    ) {
      const currentPath = [...path, fieldInfo.name];
      const origin = (element && element.elementValue.trim()) || '';
      injections.push({
        path: currentPath,
        origin,
        translation: TEXT_TODO,
      });
      return;
    }

    // List
    if (typeInfo.category === 'LIST' && typeInfoOf && element) {
      path.push((fieldInfo && fieldInfo.name) || pathNode);
      const list = element.getElements();
      const pathNodes = getPathNodes(list, typeInfoOf);
      list.forEach((li, index) => {
        parseField(path, injections, typeInfoOf, undefined, li, pathNodes[index]);
      });
      path.pop();
      return;
    }

    // Class
    if (typeInfo.category === 'CLASS' && element) {
      const attributeClass = element.getAttribute(ATTRIBUTE_NAME_CLASS);
      const classInfo: ClassInfo | undefined =
        (attributeClass && classInfoMap[attributeClass]) || classInfoMap[typeInfo.name];
      if (classInfo) {
        path.push((fieldInfo && fieldInfo.name) || pathNode);
        classInfo.fields.forEach(subFieldInfo => {
          parseField(
            path,
            injections,
            subFieldInfo.type,
            subFieldInfo,
            tryGetElement(element, subFieldInfo),
          );
        });
        path.pop();
      }
      return;
    }

    // DEBUG
    if (
      process.env.NODE_ENV === 'test' &&
      fieldInfo &&
      !noTranslate &&
      typeInfo.category === 'VALUE' &&
      typeInfo.name === TYPE_STRING
    ) {
      const currentPath = [...path, fieldInfo.name];
      const origin = (element && element.elementValue.trim()) || '';
      injections.push({
        path: currentPath,
        origin,
        translation: TEXT_TODO,
        fuzzy: true,
      });
      return;
    }
  };

  return parseField;
}

export function generateGetPathNodes(
  classInfoMap: Record<string, ClassInfo>,
): GetParseNodes {
  return (list, typeInfoOf) => {
    const classInfo: ClassInfo | undefined =
      typeInfoOf.category === 'CLASS' ? classInfoMap[typeInfoOf.name] : undefined;
    if (!classInfo) {
      return list.map((li, index) => index);
    }

    const countMap: Record<string, number[]> = {};
    const pathNodes = list.map<PathNode>((li, index) => {
      const attributeClass = li.getAttribute(ATTRIBUTE_NAME_CLASS);
      const currentClassInfo =
        (attributeClass && classInfoMap[attributeClass]) || classInfo;

      let handle: string | undefined;
      let priority = -1;
      currentClassInfo.handles.forEach(handleInfo => {
        const fieldInfo = currentClassInfo.fields.find(
          fi => fi.name === handleInfo.field,
        );

        let fieldName = handleInfo.field.replace(/^untranslated/, '');
        fieldName = `${fieldName[0].toLowerCase()}${fieldName.substring(1)}`;

        const field = li.getElement(fieldName);

        let currentHandle = (field && field.elementValue) || handleInfo.value || '';
        if (fieldInfo && fieldInfo.type.category === 'TYPE') {
          const temp = currentHandle.split('.');
          currentHandle = temp[temp.length - 1];
        }
        currentHandle = normalizeHandle(currentHandle);

        const { priority: currentPriority } = handleInfo;

        if (currentHandle && currentPriority > priority) {
          handle = currentHandle;
          priority = currentPriority;
        }
      });

      if (handle) {
        (countMap[handle] || (countMap[handle] = [])).push(index);
        return [index, handle];
      }

      return index;
    });

    pathNodes.forEach((node, index) => {
      if (Array.isArray(node)) {
        const handle = node[1];
        if (countMap[handle].length > 1) {
          const handleIndex = countMap[handle].indexOf(index);
          node[1] = `${handle}-${handleIndex}`;
        }
      }
    });

    return pathNodes;
  };
}

// ----------------------------------------------------------------
// Merging

export function pathMatch(pathA: PathNode[], pathB: PathNode[]): boolean {
  if (pathA.length !== pathB.length) {
    return false;
  }

  let result = true;

  const { length } = pathA;
  for (let i = 0; i < length; i++) {
    const nodeA = pathA[i];
    const nodeB = pathB[i];

    if (Array.isArray(nodeA) && Array.isArray(nodeB)) {
      if (nodeA[0] !== nodeB[0] || nodeA[1] !== nodeB[1]) {
        result = false;
        break;
      }
    } else if (Array.isArray(nodeA)) {
      if (
        (typeof nodeB === 'number' && nodeA[0] !== nodeB) ||
        (typeof nodeB === 'string' && nodeA[1] !== nodeB)
      ) {
        result = false;
        break;
      }
    } else if (Array.isArray(nodeB)) {
      if (
        (typeof nodeA === 'number' && nodeB[0] !== nodeA) ||
        (typeof nodeA === 'string' && nodeB[1] !== nodeA)
      ) {
        result = false;
        break;
      }
    } else if (nodeA !== nodeB) {
      result = false;
      break;
    }
  }

  return result;
}

// ----------------------------------------------------------------
// Serializing

export interface SerializedInjectionMap {
  [defType: string]: {
    [fileName: string]: string;
  };
}

export type Indent = '\t' | '  ' | '    ';
export type EndOfLine = '\n' | '\r' | '\r\n';

export function serializePath(path: PathNode[]): string {
  return path.map(node => (Array.isArray(node) ? node[1] : node)).join('.');
}

export function serialize(
  injectionMap: InjectionMap,
  indent: Indent = '  ',
  endOfLine: EndOfLine = '\n',
): SerializedInjectionMap {
  const serializedMap: SerializedInjectionMap = {};

  Object.entries(injectionMap).forEach(([defType, subInjectionMap]) => {
    serializedMap[defType] = {};
    const subSerializedMap = serializedMap[defType];
    Object.entries(subInjectionMap).forEach(([fileName, injections]) => {
      const endComments: string[] = [];
      const defTypes: string[] = [];
      const preSerialized: Record<string, string[]> = {};
      injections.forEach(injection => {
        if (typeof injection === 'string') {
          endComments.push(`${indent}<!-- ${injection} -->`);
          return;
        }
        if (injection.duplicated) {
          return;
        }

        const currentDefType = injection.path[0] as string;
        defTypes.push(currentDefType);
        const path = serializePath(injection.path);
        const block =
          preSerialized[currentDefType] || (preSerialized[currentDefType] = []);

        if (injection.unused) {
          block.push(`${indent}<!-- ${TEXT_UNUSED} -->`);
        }
        if (injection.fuzzy) {
          block.push(`${indent}<!-- ${TEXT_FUZZY} -->`);
        }

        if (Array.isArray(injection.origin)) {
          const origin = injection.origin
            .map(li => `${indent}${indent}<li>${li}</li>`)
            .join(endOfLine);
          block.push(
            `${indent}<!-- ${TEXT_EN}${endOfLine}${origin}${endOfLine}${indent}-->`,
          );
        } else {
          block.push(`${indent}<!-- ${TEXT_EN} ${injection.origin} -->`);
        }

        if (Array.isArray(injection.translation)) {
          const translation = injection.translation
            .map(li => `${indent}${indent}<li>${li}</li>`)
            .join(endOfLine);
          block.push(`${indent}<${path}>${translation}${endOfLine}${indent}</${path}>`);
        } else {
          block.push(`${indent}<${path}>${injection.translation}</${path}>`);
        }
      });

      const serializedInjections: string[] = [];

      serializedInjections.push(
        xml.DEFAULT_DECLARATION,
        `<${TAG_NAME_LANGUAGE_DATA}>`,
        '',
      );

      [...new Set(defTypes)].forEach(currentDefType => {
        serializedInjections.push(...preSerialized[currentDefType], '');
      });

      if (endComments.length > 0) {
        serializedInjections.push(...endComments, '');
      }

      serializedInjections.push(`</${TAG_NAME_LANGUAGE_DATA}>`, '');

      subSerializedMap[fileName] = serializedInjections.join(endOfLine);
    });
  });

  return serializedMap;
}

export async function save(
  directory: string,
  serializedInjectionMap: SerializedInjectionMap,
): Promise<void> {
  const promises: Promise<void>[] = [];
  Object.entries(serializedInjectionMap).forEach(([defType, subMap]) =>
    Object.entries(subMap).forEach(([fileName, doc]) =>
      promises.push(io.save(io.join(directory, defType, `${fileName}.xml`), doc)),
    ),
  );
  await Promise.all(promises);
}
