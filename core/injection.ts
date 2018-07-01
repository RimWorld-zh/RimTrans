// tslint:disable:max-func-body-length
import { Dictionary } from '../common/collection';
import { stringCompare } from '../common/utils';
import Stack from '../common/stack';
import * as logger from './logger';
import * as xml from './xml';
import * as definition from './definition';
import {
  DefSchemaType,
  FieldSchemaType,
  SchemaDefinition,
  Schema,
  schema,
} from './schema';
import config from './config';

// ==== Final Injection ====

export interface FinalInjection {
  flag?: 'duplicated' | 'nonmatched';
  path: string;
  value: string | string[];
}

// ==== Field ====

export interface Field {
  attributes: xml.Attributes;
  name: string;
  value?: string | string[];
  fields: Field[];
}

function createField(element: xml.Element, value?: string | string[]): Field {
  return {
    attributes: { ...element.attributes },
    name: element.name,
    value: value || element.value,
    fields: [],
  };
}

function fieldCompare(a: Field, b: Field): number {
  if (!a.fields && b.fields) {
    return -1;
  }
  if (a.fields && !b.fields) {
    return 1;
  }

  if (a.name === 'label') {
    return -1;
  }
  if (b.name === 'label') {
    return 1;
  }
  if (a.name === 'description') {
    return -1;
  }
  if (b.name === 'description') {
    return 1;
  }

  return stringCompare(a.name, b.name);
}

// ==== Injection ====

export interface Injection {
  attributes: xml.Attributes;
  fileName: string;
  defType: string;
  defName: string;
  fields: Field[];
  commentBefore?: string;
}

function createInjection(def: xml.Element): Injection {
  const pathNodes: string[] | undefined = def.attributes.Path
    ? def.attributes.Path.split(/\/|\\/)
    : undefined;
  const fileName: string | undefined = pathNodes
    ? pathNodes[pathNodes.length - 1]
    : undefined;

  return {
    attributes: {
      ...def.attributes,
    },
    fileName: fileName || 'unknown.xml',
    defType: def.name,
    defName: definition.getDefName(def) as string,
    fields: [],
  };
}

/**
 * Parse the xml plain text in the `DefInjected` directory, and build `InjectionData`.
 */
export function parse(rawContents: Dictionary<string>): void {
  //
}

/**
 * Inject `InjectionData` into `DefinitionData`.
 */
export function inject(): void {
  //
}

// ======== Extra ========

export function extract(defData: Dictionary<xml.Element[]>): Dictionary<Injection[]> {
  const injData: Dictionary<Injection[]> = {};
  // tslint:disable-next-line:typedef
  const addInjection = (inj: Injection) =>
    (injData[inj.defType] || (injData[inj.defType] = [])).push(inj);

  Object.entries(defData).forEach(([defType, defs]) => {
    const schemaTypeOrDefinition:
      | SchemaDefinition
      | DefSchemaType
      | undefined = (schema as Schema)[defType];
    let schemaDefinition: SchemaDefinition | undefined;

    if (schemaTypeOrDefinition === DefSchemaType.NoTranslate) {
      return;
    } else if (
      schemaTypeOrDefinition === DefSchemaType.Def ||
      schemaTypeOrDefinition === undefined
    ) {
      schemaDefinition = undefined;
    } else {
      schemaDefinition = schemaTypeOrDefinition as SchemaDefinition;
    }

    const instancedDefs: xml.Element[] = defs.filter(def => def.attributes.Instanced);

    switch (defType) {
      case 'PawnKindDef':
        instancedDefs.forEach(def => addInjection(extractInjection_PawnKindDef(def)));
        break;

      default:
        instancedDefs.forEach(def =>
          addInjection(extractInjection(def, schemaDefinition)),
        );
    }
  });

  return injData;
}

// ==== Extract Injection ====

function extractInjection(
  def: xml.Element,
  schemaDefinition?: SchemaDefinition,
): Injection {
  const defName: string = definition.getDefName(def) as string;
  const injection: Injection = createInjection(def);

  extractInjectionRecursively(def, injection, schema.Def);

  if (schemaDefinition) {
    extractInjectionRecursively(def, injection, schemaDefinition);
  }

  return injection;
}

function extractInjectionRecursively(
  element: xml.Element,
  field: Injection | Field,
  schemaDefinition: SchemaDefinition,
): void {
  const fields: Field[] = [];

  Object.entries(schemaDefinition).forEach(([name, childSchemaDefinition]) => {
    const childElement: xml.Element | undefined = element.nodes.find(
      xml.isElementByName(name),
    );
    switch (typeof childSchemaDefinition) {
      case 'boolean': // normal extracting
        if (childElement && childSchemaDefinition) {
          fields.push(createField(childElement));
        }
        break;

      case 'string': // this means that the field has default value in the assembly.
        if (childElement) {
          fields.push(createField(childElement));
        } else {
          fields.push({
            attributes: {},
            name,
            value: childSchemaDefinition as string,
            fields: [],
          });
        }
        break;

      case 'object': // has child fields
        if (childElement) {
          if (name === 'li') {
            element.nodes.filter(xml.isElementByName('li')).forEach(li => {
              const childField: Field = createField(li);
              extractInjectionRecursively(
                li,
                childField,
                childSchemaDefinition as SchemaDefinition,
              );
              if (childField.fields.length > 0) {
                fields.push(childField);
              }
            });
          } else {
            const childField: Field = createField(childElement);
            extractInjectionRecursively(
              childElement,
              childField,
              childSchemaDefinition as SchemaDefinition,
            );
            if (childField.fields.length > 0) {
              fields.push(childField);
            }
          }
        }
        break;

      default:
        // some specific extracting mode
        switch (childSchemaDefinition) {
          case FieldSchemaType.TranslationCanChangeCount:
            if (childElement) {
              fields.push(
                createField(
                  childElement,
                  childElement.nodes
                    .filter(xml.isElementByName('li'))
                    .map(li => li.value || ''),
                ),
              );
            }
            break;

          case FieldSchemaType.SameToLabel:
            if (childElement) {
              fields.push(createField(childElement));
            } else {
              const label: xml.Element | undefined = element.nodes.find(
                xml.isElementByName('label'),
              );
              fields.push({
                attributes: {},
                name,
                value: (label && label.value) || '',
                fields: [],
              });
            }
            break;

          default:
        }
    }
  });

  field.fields.push(...fields.sort(fieldCompare));
}

// ==== PawnKindDef ====
const genderLabels: ReadonlyArray<string> = [
  'labelMale',
  'labelMalePlural',
  'labelFemale',
  'labelFemalePlural',
];

function extractInjection_PawnKindDef(def: xml.Element): Injection {
  // console.log(JSON.stringify(def, undefined, '  '));
  const defName: string = definition.getDefName(def) as string;
  const injection: Injection = createInjection(def);
  console.log(defName, def.attributes.HasGenders, def.attributes.Humanlike);

  const hasGenders: boolean = !!def.attributes.HasGenders;
  const isHumanlike: boolean = !!def.attributes.Humanlike;

  const lifeStages: xml.Element | undefined = def.nodes.find(
    xml.isElementByName('lifeStages'),
  );
  const hasGenderLabels: boolean =
    hasGenders &&
    (genderLabels.some(gl => def.nodes.some(xml.isElementByName(gl))) ||
      (!!lifeStages &&
        lifeStages.nodes
          .filter(xml.isElementByName('li'))
          .some(ls => genderLabels.some(gl => ls.nodes.some(xml.isElementByName(gl))))));

  const label: xml.Element | undefined = def.nodes.find(xml.isElementByName('label'));
  const labelValue: string = (label && label.value) || defName;
  injection.fields.push(
    label
      ? createField(label)
      : {
          attributes: {},
          name: 'label',
          value: labelValue,
          fields: [],
        },
  );
  const labelPlural: xml.Element | undefined = def.nodes.find(
    xml.isElementByName('labelPlural'),
  );
  injection.fields.push(
    labelPlural
      ? createField(labelPlural)
      : {
          attributes: {},
          name: 'labelPlural',
          value: labelValue,
          fields: [],
        },
  );

  const description: xml.Element | undefined = def.nodes.find(
    xml.isElementByName('description'),
  );
  if (description) {
    injection.fields.push(createField(description));
  }

  if (hasGenderLabels || (hasGenders && !isHumanlike)) {
    // console.log('gender labels', defName);
    let isOdd: boolean = true;
    let labelCache: string | undefined;
    genderLabels.forEach(gl => {
      const element: xml.Element | undefined = def.nodes.find(xml.isElementByName(gl));
      if (element && isOdd) {
        labelCache = element.value;
      }
      injection.fields.push(
        element
          ? createField(element)
          : {
              attributes: {},
              name: gl,
              value: (!isOdd && labelCache) || labelValue,
              fields: [],
            },
      );
      isOdd = !isOdd;
    });
  }
  if (lifeStages) {
    const lifeStagesField: Field = createField(lifeStages);
    lifeStages.nodes.filter(xml.isElementByName('li')).forEach(ls => {
      if (!ls.attributes.Visible) {
        return;
      }

      const lsField: Field = createField(ls);
      const lsLabel: xml.Element | undefined = ls.nodes.find(
        xml.isElementByName('label'),
      );
      const lsLabelValue: string = (lsLabel && lsLabel.value) || labelValue;
      lsField.fields.push(
        lsLabel
          ? createField(lsLabel)
          : {
              attributes: {},
              name: 'label',
              value: lsLabelValue,
              fields: [],
            },
      );
      const lsLabelPlural: xml.Element | undefined = ls.nodes.find(
        xml.isElementByName('labelPlural'),
      );
      lsField.fields.push(
        lsLabelPlural
          ? createField(lsLabelPlural)
          : {
              attributes: {},
              name: 'labelPlural',
              value: lsLabelValue,
              fields: [],
            },
      );

      if (hasGenderLabels || (hasGenders && !isHumanlike)) {
        // console.log('life stage gender labels', defName);
        let isOdd: boolean = true;
        let labelCache: string | undefined;
        genderLabels.forEach(gl => {
          const element: xml.Element | undefined = ls.nodes.find(xml.isElementByName(gl));
          if (element && isOdd) {
            labelCache = element.value;
          }
          lsField.fields.push(
            element
              ? createField(element)
              : {
                  attributes: {},
                  name: gl,
                  value: (!isOdd && labelCache) || labelValue,
                  fields: [],
                },
          );
          isOdd = !isOdd;
        });
      }

      lifeStagesField.fields.push(lsField);
    });
    if (lifeStagesField.fields.length > 0) {
      injection.fields.push(lifeStagesField);
    }
  }

  return injection;
}

// ======== Generate XML text ========

// In the text list, empty string will be converted to empty line.

export function generateXMLContents(data: Dictionary<Injection[]>): Dictionary<string> {
  const interimData: Dictionary<string[]> = {};
  Object.entries(data).forEach(([defType, injs]) => {
    injs.forEach(inj => {
      const path: string = `${defType}/${inj.fileName}`;
      const textList: string[] = interimData[path] || (interimData[path] = []);
      const textListToPush: string[] = generateTextList(inj);
      textListToPush.length > 1 && textList[textList.length - 1]
        ? textList.push('', ...textListToPush)
        : textList.push(...textListToPush);
    });
  });

  const rawContents: Dictionary<string> = {};
  Object.entries(interimData).forEach(([path, textList]) => {
    if (textList.length > 0) {
      textList[0]
        ? textList.unshift(
            '<?xml version="1.0" encoding="utf-8" ?>',
            '<LanguageData>',
            '',
          )
        : textList.unshift('<?xml version="1.0" encoding="utf-8" ?>', '<LanguageData>');
      textList[textList.length - 1]
        ? textList.push('', '', '</LanguageData>')
        : textList.push('', '</LanguageData>');
      rawContents[path] = textList.join(config.eol);
    }
  });

  return rawContents;
}

function generateTextList(inj: Injection): string[] {
  const textList: string[] = [];
  const fieldStack: Stack<Field> = new Stack<Field>();
  const pathStack: Stack<string> = new Stack<string>().push(inj.defName);

  // tslint:disable-next-line:typedef
  const fieldToText = (field: Field) => {
    fieldStack.push(field);
    pathStack.push(field.name === 'li' ? `${field.attributes.Index}` : field.name);

    if (field.value) {
      const key: string = pathStack.items.join('.');
      if (typeof field.value === 'string') {
        textList.push(`${config.indent}<${key}>${field.value}</${key}>`);
      } else if (Array.isArray(field.value)) {
        textList.push(`${config.indent}<${key}>`);
        textList.push(
          ...field.value.map(v => `${config.indent}${config.indent}<li>${v}</li>`),
        );
        textList.push(`${config.indent}</${key}>`);
      } else {
        // TODO
      }
    } else {
      field.fields.forEach(fieldToText);
    }

    fieldStack.pop();
    pathStack.pop();
  };

  inj.fields.forEach(fieldToText);

  if (inj.attributes.Comment) {
    textList.unshift('', `${config.indent}<!--${inj.attributes.Comment}-->`, '');
  } else if (textList.length > 1) {
    textList.push('');
  }

  return textList;
}
