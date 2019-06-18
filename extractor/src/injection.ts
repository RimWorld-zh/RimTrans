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
  ATTRIBUTE_TRANSLATION_CAN_CHANGE_COUNT,
  TYPE_STRING,
} from './type-package';

xml.mountDomPrototype();

// CONSTANTS
const TAG_NAME_LI = 'li';
const ATTRIBUTE_NAME_CLASS = 'Class';
const TEXT_TODO = 'TODO';

/**
 * The type of path nodes, if is tuple `[number, string]`, means `[index, handle]`
 */
export type PathNode = string | number | [number, string];

export interface Injection {
  path: PathNode[];
  origin: string | string[];
  translation: string | string[];
  duplicated: boolean;
}

export interface InjectionMap {
  [defType: string]: {
    [fileName: string]: (Injection | string)[];
  };
}

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

export function getTextValue(element: Element): string {
  return (element.elementValue && element.elementValue.trim()).replace(/\n/g, '\\n');
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
    const defNameElement = def.getElement('defName');
    const defName =
      defNameElement && defNameElement.elementValue && defNameElement.elementValue.trim();
    const classInfo: ClassInfo | undefined =
      classInfoMap[def.tagName] || classInfoMap.Def;

    if (!defName) {
      return;
    }

    classInfo.fields.forEach(fieldInfo => {
      parseField(
        [defName],
        injections,
        fieldInfo.type,
        fieldInfo,
        def.getElement(fieldInfo.name),
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

    // skip
    if (
      fieldInfo &&
      (fieldInfo.attributes.includes(ATTRIBUTE_UNSAVED) ||
        fieldInfo.attributes.includes(ATTRIBUTE_NO_TRANSLATE))
    ) {
      return;
    }

    // List<string>
    if (
      fieldInfo &&
      fieldInfo.attributes.includes(ATTRIBUTE_MUST_TRANSLATE) &&
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
        origin.push(getTextValue(li));
        translation.push(TEXT_TODO);
      });
      injections.push({
        path: currentPath,
        origin,
        translation,
        duplicated: false,
      });
      return;
    }

    // String
    if (
      fieldInfo &&
      (fieldInfo.attributes.includes(ATTRIBUTE_MUST_TRANSLATE) ||
        fieldInfo.attributes.includes(ATTRIBUTE_MAY_TRANSLATE)) &&
      typeInfo.category === 'VALUE' &&
      typeInfo.name === TYPE_STRING
    ) {
      const currentPath = [...path, fieldInfo.name];
      const origin = (element && getTextValue(element)) || '';
      injections.push({
        path: currentPath,
        origin,
        translation: TEXT_TODO,
        duplicated: false,
      });
      return;
    }

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
            element.getElement(subFieldInfo.name),
          );
        });
        path.pop();
      }
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
        let fieldName = handleInfo.field.replace(/^untranslated/, '');
        fieldName = `${fieldName[0].toLowerCase()}${fieldName.substring(1)}`;
        const field = li.getElement(fieldName);
        const currentHandle = normalizeHandle(
          handleInfo.value || (field && field.elementValue) || '',
        );
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
