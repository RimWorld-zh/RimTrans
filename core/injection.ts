// tslint:disable:max-func-body-length

import * as logger from './logger';
import * as xml from './xml';
import * as definition from './definition';
import { RawContents, stringCompare } from './utils';
import {
  DefSchemaType,
  FieldSchemaType,
  SchemaDefinition,
  Schema,
  schema,
} from './schema';

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
    value: value || element.value || '',
    fields: [],
  };
}

// ==== Injection ====

export interface Injection {
  attributes: xml.Attributes;
  fileName?: string;
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
    fileName,
    defType: def.name,
    defName: definition.getDefName(def) as string,
    fields: [],
  };
}

export interface InjectionData {
  [defType: string]: Injection[];
}

/**
 * Parse the xml plain text in the `DefInjected` directory, and build `InjectionData`.
 */
export function parse(rawContents: RawContents): void {
  //
}

/**
 * Inject `InjectionData` into `DefinitionData`.
 */
export function inject(): void {
  //
}

/**
 * Walk the `DefinitionData` and extra `InjectionData`.
 */
export function extract(defData: definition.DefinitionData): InjectionData {
  const injData: InjectionData = {};
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

    defs.forEach(def => {
      if (
        def.attributes.Abstract === 'True' ||
        !def.nodes.some(xml.isElementByName('defName'))
      ) {
        return;
      }
      addInjection(extractInjection(def, schemaDefinition));
    });
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

      default: // some specific extracting mode
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

  field.fields.push(
    ...fields.sort((a, b) => {
      if (!a.fields && b.fields) {
        return -1;
      }
      if (a.fields && !b.fields) {
        return 1;
      }

      return stringCompare(a.name, b.name);
    }),
  );
}
