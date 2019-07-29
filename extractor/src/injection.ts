import * as io from '@rimtrans/io';
import {
  ATTRIBUTE_UNSAVED,
  ATTRIBUTE_NO_TRANSLATE,
  ATTRIBUTE_MUST_TRANSLATE,
  ATTRIBUTE_MAY_TRANSLATE,
  ATTRIBUTE_LOAD_ALIAS,
  ATTRIBUTE_TRANSLATION_CAN_CHANGE_COUNT,
  TYPE_STRING,
  ATTRIBUTE_NAME_ABSTRACT,
} from './constants';
import { ExtractorEventEmitter, Progress } from './extractor-event-emitter';
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
import { cloneObject } from './object';
import { DefsElementMap } from './definition';
import { ClassInfo, HandleInfo, EnumInfo, FieldInfo, TypeInfo } from './type-package';

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

export interface SerializedInjectionMap {
  [defType: string]: {
    [fileName: string]: string;
  };
}

type ParseDef = (injectionList: Injection[], def: XElementData) => void;

type ParseField = (
  path: PathNode[],
  injectionList: Injection[],
  typeInfo: TypeInfo,
  fieldInfo?: FieldInfo,
  element?: XElementData,
  pathNode?: PathNode,
) => void;

type GetParseNodes = (list: XElementData[], typeInfoOf: TypeInfo) => PathNode[];

export class InjectionExtractor {
  /* eslint-disable lines-between-class-members */
  public readonly ACTION_LOAD = 'DefInjected Load';
  public readonly ACTION_SAVE = 'DefInjected Save';
  /* eslint-enable lines-between-class-members */

  private readonly emitter: ExtractorEventEmitter;

  public constructor(emitter: ExtractorEventEmitter) {
    this.emitter = emitter;
  }

  // ----------------------------------------------------------------
  // Basic functions

  private tryGetElement(
    parent: XElementData,
    fieldInfo: FieldInfo,
  ): XElementData | undefined {
    let element: XElementData | undefined = parent.elements.find(
      c => c.name === fieldInfo.name,
    );
    if (!element && fieldInfo.alias) {
      element = parent.elements.find(c => c.name === fieldInfo.alias);
    }
    return element;
  }

  public normalizeHandle(text: string): string {
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
   * Detect tow paths is matched or not.
   * @param pathA the first path
   * @param pathB the second path
   */
  public pathMatch(pathA: PathNode[], pathB: PathNode[]): boolean {
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

  /**
   * Serialize a path to text for xml tag name.
   * @param path the path
   */
  public serializePath(path: PathNode[]): string {
    return path.map(node => (Array.isArray(node) ? node[1] : node)).join('.');
  }

  private readonly REGEX_INTEGER = /^\d+$/;

  public deSerializePath(tagName: string): PathNode[] {
    return tagName
      .split('.')
      .map(node => (this.REGEX_INTEGER.test(node) ? Number.parseInt(node, 10) : node));
  }

  // ----------------------------------------------------------------
  // Loading

  /**
   * Load `DefInjected` xml files of mods and get array of `InjectionMap`.
   * @param defInjectedDirectories the array of paths to `DefInjected` directories, `[Core, ...Mods]`
   */
  public async load(defInjectedDirectories: string[]): Promise<InjectionMap[]> {
    const action = this.ACTION_LOAD;

    return Promise.all(
      defInjectedDirectories.map(async dir => {
        this.emitter.emit('progress', {
          action,
          key: dir,
          status: 'pending',
          info: 'loading',
        });

        const map: InjectionMap = {};

        const fileMaps: Record<string, string[]> = {};
        const files = await io.search(['*/*.xml'], {
          cwd: dir,
          onlyFiles: true,
          case: false,
        });

        if (files.length === 0) {
          this.emitter.emit('progress', {
            action,
            key: dir,
            status: 'succeed',
            info: 'no files',
          });
          return map;
        }

        await Promise.all(
          files.map(async relatedPath => {
            const defType = io.directoryName(relatedPath);
            const fileName = io.fileName(relatedPath, true);
            const path = io.join(dir, relatedPath);
            const subMap = map[defType] || (map[defType] = {});
            subMap[fileName] = await this.loadFile(path);
          }),
        );

        this.emitter.emit('progress', {
          action,
          key: dir,
          status: 'succeed',
          info: 'loaded',
        });

        return map;
      }),
    );
  }

  private async loadFile(path: string): Promise<(string | Injection)[]> {
    const root = await loadXML(path);
    const injectionList: (string | Injection)[] = [];

    let comment = '';
    let origin: string | string[] = '';
    let translation: string | string[] = TEXT_TODO;
    root.childNodes.forEach(node => {
      switch (node.nodeType) {
        case 'comment':
          comment = node.value.trim();
          if (comment === TEXT_UNUSED) {
            // do nothing
          } else if (comment.startsWith(TEXT_EN)) {
            origin = comment.replace(TEXT_EN, '').trim();
          } else {
            injectionList.push(comment);
          }
          break;

        case 'element':
          translation = node.elements.map(li => li.value.trim());
          if (translation.length === 0) {
            translation = node.value;
          }
          injectionList.push({
            path: this.deSerializePath(node.name),
            origin,
            translation,
          });
          break;

        default:
      }
    });

    return injectionList;
  }

  // ----------------------------------------------------------------
  // Parsing

  /**
   * Parse the Def maps and get Def injection maps.
   * @param defsElementMaps the array of Def maps
   * @param typeInfoMaps the array of DefInjection Map
   * @param fuzzy extract injection in fuzzy mode or not
   */
  public parse(
    defsElementMaps: DefsElementMap[],
    classInfoMap: Record<string, ClassInfo>,
    fuzzy?: boolean,
  ): InjectionMap[] {
    const getPathNodes = this.generateGetPathNodes(classInfoMap);
    const parseField = this.generateParseField(classInfoMap, getPathNodes, fuzzy);
    const parseDef = this.generateParseDef(classInfoMap, parseField);

    const result: InjectionMap[] = defsElementMaps.map(defMap => {
      const injectionMap: InjectionMap = {};

      Object.entries(defMap).forEach(([path, root]) => {
        const filename = io.fileName(path);
        root.elements.forEach(def => {
          const defType = def.name;
          const injectionFileMap = injectionMap[defType] || (injectionMap[defType] = {});
          const injectionList =
            injectionFileMap[filename] || (injectionFileMap[filename] = []);

          parseDef(injectionList as Injection[], def);
        });
      });

      return injectionMap;
    });

    return result;
  }

  // ----------------------------------------------------------------
  // Internal parsing functions

  /**
   * High-order function: generate a function for parsing a Def element and getting `Injection`.
   * @param classInfoMap the `ClassInfo` map
   * @param parseField the `ParseField` function
   */
  private generateParseDef(
    classInfoMap: Record<string, ClassInfo>,
    parseField: ParseField,
  ): ParseDef {
    return (injectionList, def) => {
      const abstract = def.attributes[ATTRIBUTE_NAME_ABSTRACT];
      const defNameElement = def.elements.find(c => c.name === FIELD_NAME_DEF_NAME);
      const defName = defNameElement && defNameElement.value.trim();
      const classInfo: ClassInfo | undefined = classInfoMap[def.name] || classInfoMap.Def;

      if ((abstract && abstract.toLowerCase() === 'true') || !defName) {
        return;
      }

      classInfo.fields.forEach(fieldInfo => {
        if (fieldInfo.name === FIELD_NAME_DEF_NAME) {
          return;
        }
        parseField(
          [defName],
          injectionList,
          fieldInfo.type,
          fieldInfo,
          this.tryGetElement(def, fieldInfo),
        );
      });
    };
  }

  /**
   * High-order function: generate a function for parsing a field element and getting `Injection`
   * @param classInfoMap the `ClassInfo` map
   * @param getPathNodes the `GetPathNodes` function
   * @param fuzzy extract injections in fuzzy mode or not
   */
  private generateParseField(
    classInfoMap: Record<string, ClassInfo>,
    getPathNodes: GetParseNodes,
    fuzzy?: boolean,
  ): ParseField {
    /**
     * @param path
     * @param injectionList
     * @param typeInfo
     * @param fieldInfo if is undefined, means this is a list item
     * @param element
     * @param pathNode
     */
    const parseField: ParseField = (
      path,
      injectionList,
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
        element.elements.forEach(li => {
          origin.push(li.value.trim());
          translation.push(TEXT_TODO);
        });
        injectionList.push({
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
        const origin = (element && element.value.trim()) || '';
        injectionList.push({
          path: currentPath,
          origin,
          translation: TEXT_TODO,
        });
        return;
      }

      // List
      if (typeInfo.category === 'LIST' && typeInfoOf && element) {
        path.push((fieldInfo && fieldInfo.name) || pathNode);
        const list = element.elements;
        const pathNodes = getPathNodes(list, typeInfoOf);
        list.forEach((li, index) => {
          parseField(path, injectionList, typeInfoOf, undefined, li, pathNodes[index]);
        });
        path.pop();
        return;
      }

      // Class
      if (typeInfo.category === 'CLASS' && element) {
        const attributeClass = element.attributes[ATTRIBUTE_NAME_CLASS];
        const classInfo: ClassInfo | undefined =
          (attributeClass && classInfoMap[attributeClass]) || classInfoMap[typeInfo.name];
        if (classInfo) {
          path.push((fieldInfo && fieldInfo.name) || pathNode);
          classInfo.fields.forEach(subFieldInfo => {
            parseField(
              path,
              injectionList,
              subFieldInfo.type,
              subFieldInfo,
              this.tryGetElement(element, subFieldInfo),
            );
          });
          path.pop();
        }
        return;
      }

      // Fuzzy
      if (
        fuzzy &&
        fieldInfo &&
        !noTranslate &&
        typeInfo.category === 'VALUE' &&
        typeInfo.name === TYPE_STRING
      ) {
        const origin = (element && element.value.trim()) || '';
        if (origin.includes(' ')) {
          const currentPath = [...path, fieldInfo.name];
          injectionList.push({
            path: currentPath,
            origin,
            translation: TEXT_TODO,
            fuzzy: true,
          });
        }
      }
    };

    return parseField;
  }

  /**
   * High-order function: generate a `GetPathNodes` function
   * @param classInfoMap the map of ClassInfo
   */
  private generateGetPathNodes(classInfoMap: Record<string, ClassInfo>): GetParseNodes {
    return (list, typeInfoOf) => {
      const classInfo: ClassInfo | undefined =
        typeInfoOf.category === 'CLASS' ? classInfoMap[typeInfoOf.name] : undefined;
      if (!classInfo) {
        return list.map((li, index) => index);
      }

      const countMap: Record<string, number[]> = {};
      const pathNodes = list.map<PathNode>((li, index) => {
        const attributeClass = li.attributes[ATTRIBUTE_NAME_CLASS];
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

          const field = li.elements.find(c => c.name === fieldName);

          let currentHandle = (field && field.value) || handleInfo.value || '';
          if (fieldInfo && fieldInfo.type.category === 'TYPE') {
            const temp = currentHandle.split('.');
            currentHandle = temp[temp.length - 1];
          }
          currentHandle = this.normalizeHandle(currentHandle);

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

  /**
   * Merge the data of all of a `InjectionMap` to a target `InjectionMap`, return a new `InjectionMap`.
   * @param target the target `InjectionMap` to merge into
   * @param source the source `InjectionMap`
   */
  public merge(target: InjectionMap, source: InjectionMap): InjectionMap {
    const targetMap: InjectionMap = cloneObject(target);
    const sourceMap: InjectionMap = cloneObject(source);

    Object.entries(sourceMap).forEach(([defType, subSourceMap]) => {
      const subTargetMap = targetMap[defType] || (targetMap[defType] = {});
      Object.entries(subSourceMap).forEach(([fileName, sourceInjectionList]) => {
        const targetInjectionList =
          subTargetMap[fileName] || (subTargetMap[fileName] = []);
        sourceInjectionList.forEach(sourceInjection => {
          if (typeof sourceInjection === 'string') {
            targetInjectionList.push(sourceInjection);
            return;
          }
          const targetInjection = targetInjectionList.find(
            (inj): inj is Injection =>
              typeof inj === 'object' && this.pathMatch(inj.path, sourceInjection.path),
          );
          if (!targetInjection) {
            sourceInjection.unused = true;
            targetInjectionList.push(sourceInjection);
            return;
          }
          if (
            Array.isArray(targetInjection.translation) &&
            Array.isArray(sourceInjection.translation)
          ) {
            sourceInjection.translation.forEach((sourceText, index) => {
              if (sourceText && sourceText !== TEXT_TODO) {
                (targetInjection.translation as string[])[index] = sourceText;
              }
            });
          } else if (
            typeof targetInjection.translation === 'string' &&
            typeof sourceInjection.translation === 'string' &&
            sourceInjection.translation &&
            sourceInjection.translation !== TEXT_TODO &&
            sourceInjection.translation !== targetInjection.origin
          ) {
            targetInjection.translation = sourceInjection.translation;
          }
        });
      });
    });

    return targetMap;
  }

  // ----------------------------------------------------------------
  // Check duplicated

  /**
   * Check all of injections is duplicated or not, should be call after `merge()`
   * @param injectionMaps the array of `InjectionMap`, `[Core, ...Mods]`
   */
  public checkDuplicated(injectionMaps: InjectionMap[]): InjectionMap[] {
    injectionMaps.forEach(map =>
      Object.keys(map)
        .sort()
        .forEach(defType => {
          const subMap = map[defType];
          const visited: Injection[] = [];
          Object.entries(subMap)
            .sort(([a], [b]) => a.localeCompare(b))
            .forEach(([fileName, injectionList]) =>
              injectionList.forEach(injection => {
                if (typeof injection === 'string') {
                  return;
                }
                if (visited.some(inj => this.pathMatch(inj.path, injection.path))) {
                  injection.duplicated = true;
                  return;
                }
                visited.push(injection);
              }),
            );
        }),
    );

    return injectionMaps;
  }

  // ----------------------------------------------------------------
  // Saving

  /**
   * Save `InjectionMap` as XML document files.
   * @param directory the directory to save to
   * @param injectionMap the `InjectionMap` to serialize
   * @param prettierOptions format options
   */
  public async save(
    directory: string,
    injectionMap: InjectionMap,
    prettierOptions?: PrettierOptions,
  ): Promise<void> {
    const action = this.ACTION_SAVE;
    this.emitter.emit('progress', { action, key: '', status: 'pending', info: 'saving' });

    const injectionListMap: Record<string, (Injection | string)[]> = {};
    Object.entries(injectionMap).forEach(([defType, subInjectionMap]) =>
      Object.entries(subInjectionMap).forEach(([fileName, injectionList]) => {
        const path = io.join(directory, defType, `${fileName}.xml`);
        injectionListMap[path] = injectionList;
      }),
    );

    Object.entries(injectionMap).forEach(([defType, subInjectionMap]) =>
      Object.entries(subInjectionMap).forEach(([fileName, injectionList]) => {}),
    );

    await Promise.all(
      Object.entries(injectionListMap).map(async ([path, injectionList]) =>
        this.saveFile(path, injectionList, prettierOptions),
      ),
    );

    this.emitter.emit('progress', { action, key: '', status: 'succeed', info: 'saved' });
  }

  private async saveFile(
    path: string,
    injectionList: (string | Injection)[],
    prettierOptions?: PrettierOptions,
  ): Promise<void> {
    const { tab, indent, eol, newline } = resolveXmlPrettierOptions(prettierOptions);

    const defTypes: string[] = [];
    const preSerialized: Record<string, XNodeData[]> = {};
    const endComments: XNodeData[] = [];

    injectionList.forEach(injection => {
      if (typeof injection === 'string') {
        endComments.push(
          indent,
          {
            nodeType: 'comment',
            value: ` ${injection} `,
          },
          newline,
        );
        return;
      }
      if (injection.duplicated) {
        return;
      }

      const currentDefType = injection.path[0] as string;
      defTypes.push(currentDefType);
      const tagName = this.serializePath(injection.path);
      const block = preSerialized[currentDefType] || (preSerialized[currentDefType] = []);

      if (injection.fuzzy) {
        block.push(
          indent,
          {
            nodeType: 'comment',
            value: ` ${TEXT_FUZZY} `,
          },
          newline,
        );
      }
      if (injection.unused) {
        block.push(
          indent,
          {
            nodeType: 'comment',
            value: ` ${TEXT_UNUSED} `,
          },
          newline,
        );
      }

      if (Array.isArray(injection.origin)) {
        const origin = injection.origin.map(li => `${tab}${tab}<li>${li}</li>`).join(eol);
        block.push(
          indent,
          {
            nodeType: 'comment',
            value: ` ${TEXT_EN}${eol}${origin}${eol}${tab}`,
          },
          newline,
        );
      } else {
        block.push(
          indent,
          {
            nodeType: 'comment',
            value: ` ${TEXT_EN} ${injection.origin} `,
          },
          newline,
        );
      }

      if (Array.isArray(injection.translation)) {
        const list = injection.translation.map<XElementData>(li => ({
          nodeType: 'element',
          name: TAG_NAME_LI,
          attributes: {},
          childNodes: [{ nodeType: 'text', value: li }],
          elements: [],
          value: '',
        }));

        block.push(
          indent,
          {
            nodeType: 'element',
            name: tagName,
            attributes: {},
            childNodes: list.reduce<XNodeData[]>(
              (agg, el) => {
                agg.push(indent, el, newline, indent);
                return agg;
              },
              [newline, indent],
            ),
            elements: list,
            value: '',
          },
          newline,
        );
      } else {
        block.push(
          indent,
          {
            nodeType: 'element',
            name: tagName,
            attributes: {},
            childNodes: [{ nodeType: 'text', value: injection.translation }],
            elements: [],
            value: injection.translation,
          },
          newline,
        );
      }
    });

    const childNodes: XNodeData[] = [newline, newline];

    [...new Set(defTypes)].forEach(currentDefType => {
      childNodes.push(...preSerialized[currentDefType], newline);
    });

    if (endComments.length > 0) {
      childNodes.push(...endComments, newline);
    }

    const languageData: XElementData = {
      nodeType: 'element',
      name: TAG_NAME_LANGUAGE_DATA,
      attributes: {},
      childNodes,
      elements: childNodes.filter((c): c is XElementData => c.nodeType === 'element'),
      value: '',
    };

    await saveXML(path, languageData, false, prettierOptions);
  }
}
