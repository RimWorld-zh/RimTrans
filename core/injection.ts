/**
 * Injection
 */

import * as logger from './logger';
import * as xml from './xml';
import * as definition from './definition';
import { RawContents, stringCompare } from './utils';
import {
  schema,
  DefSchemaType,
  Schema,
  SchemaDefinition,
  FieldSchemaType,
} from './schema';

export interface Field {
  attributes: xml.Attributes;
  name: string;
  value?: string | string[];
  fields?: Field[];
}

export interface Injection {
  attributes: xml.Attributes;
  defType: string;
  defName: string;
  fields: Field[];
  commentBefore?: string;
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
export function extract(defData: definition.DefinitionData): void {
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
    let isSpecialBodyDef: boolean = false;

    if (schemaTypeOrDefinition === DefSchemaType.NoTranslate) {
      return;
    } else if (schemaTypeOrDefinition === DefSchemaType.SpecialBodyDef) {
      isSpecialBodyDef = true;
    } else if (
      schemaTypeOrDefinition === DefSchemaType.Def ||
      schemaTypeOrDefinition === undefined
    ) {
      schemaDefinition = undefined;
    } else {
      schemaDefinition = schemaTypeOrDefinition as SchemaDefinition;
    }

    if (isSpecialBodyDef) {
      defs.forEach(def => {
        if (
          def.attributes.Abstract === 'True' ||
          !def.nodes.some(xml.isElementByName('defName'))
        ) {
          return;
        }
        addInjection(extractInjectionSpecial_BodyDef(def));
      });
    } else {
      defs.forEach(def => {
        if (
          def.attributes.Abstract === 'True' ||
          !def.nodes.some(xml.isElementByName('defName'))
        ) {
          return;
        }
        addInjection(extractInjection(def, schemaDefinition));
      });
    }
  });
}

// ==== Extract Injection ====

function extractInjection(
  def: xml.Element,
  schemaDefinition?: SchemaDefinition,
): Injection {
  const defName: string = definition.getDefName(def) as string;
  const injection: Injection = {
    attributes: {
      ...def.attributes,
    },
    defType: def.name,
    defName,
    fields: [],
  };

  ['label', 'description'].forEach(name => {
    const element: xml.Element | undefined = def.nodes.find(xml.isElementByName(name));
    if (element) {
      injection.fields.push({
        attributes: {},
        name,
        value: element.value || '',
      });
    }
  });

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
      case 'boolean':
        if (childElement && childSchemaDefinition) {
          fields.push({
            attributes: {},
            name,
            value: childElement.value || '',
          });
        }
        break;

      case 'string':
        fields.push({
          attributes: {},
          name,
          value:
            (childElement && childElement.value) || (childSchemaDefinition as string),
        });
        break;

      case 'object':
        if (childElement) {
          const childField: Field = {
            attributes: {
              Index: childElement.attributes.Index,
            },
            name,
          };
          extractInjectionRecursively(
            childElement,
            childField,
            childSchemaDefinition as SchemaDefinition,
          );
        }
        break;

      default:
        if (
          childElement &&
          childSchemaDefinition === FieldSchemaType.TranslationCanChangeCount
        ) {
          fields.push({
            attributes: {},
            name,
            value: childElement.nodes
              .filter(xml.isElementByName('li'))
              .map(li => li.value || ''),
          });
        }
    }

    fields.sort((a, b) => {
      if (!a.fields && b.fields) {
        return -1;
      }
      if (a.fields && !b.fields) {
        return 1;
      }

      return stringCompare(a.name, b.name);
    });
    field.fields = field.fields ? field.fields.concat(fields) : fields;
  });
}

// ==== Extract Injection Special BodyDef ====

function extractInjectionSpecial_BodyDef(def: xml.Element): Injection {
  const injection: Injection = extractInjection(def);

  const corePartElement: xml.Element | undefined = def.nodes.find(
    xml.isElementByName('corePart'),
  );
  if (corePartElement) {
    const corePartField:
      | Field
      | undefined = extractInjectionSpecial_BodyPartRecordRecursively(corePartElement);
    if (corePartField) {
      (injection.fields || (injection.fields = [])).push(corePartField);
    }
  }

  return injection;
}

function extractInjectionSpecial_BodyPartRecordRecursively(
  element: xml.Element,
): Field | undefined {
  const field: Field = {
    attributes: {},
    name: element.name,
  };

  const customLabel: xml.Element | undefined = element.nodes.find(
    xml.isElementByName('customLabel'),
  );
  if (customLabel) {
    (field.fields || (field.fields = [])).push({
      attributes: {},
      name: 'customLabel',
      value: customLabel.value || '',
    });
  }

  const partsElement: xml.Element | undefined = element.nodes.find(
    xml.isElementByName('parts'),
  );
  if (partsElement) {
    const partsField: Field = {
      attributes: {},
      name: 'parts',
    };
  }

  if (field.fields && field.fields.length > 0) {
    return field;
  } else {
    return undefined;
  }
}
