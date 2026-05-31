/**
 * The meta info of the assembly. Includes enums, classes and defs.
 */
export interface AssemblyMeta {
  /**
   * The name of the assembly.
   */
  name: string;
  /**
   * All enum types.
   */
  enums: Record<string, EnumMeta>;
  /**
   * All class types.
   */
  classes: Record<string, ClassMeta>;
  /**
   * The map for defs name to full name.
   */
  defs: Record<string, string>;
}

/**
 * The meta info of the enum.
 */
export interface EnumMeta {
  name: string;
  values: Record<string, number>;
}

/**
 * The meta info of the class.
 */
export interface ClassMeta {
  name: string;
  fields: FieldMeta[];
  attrs: Record<string, Record<string, any>>;
}

/**
 * Meta info of the field.
 */
export interface FieldMeta {
  name: string;
  type: TypeMeta;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  attrs: Record<string, Record<string, any>>;
}

export type TypeCategory =
  | 'Type' // Type type in reflection
  | 'Value' // The value types (in javascript) like 'string', 'int', 'float' or 'bool'
  | 'Enum' // Enum type
  | 'List' // Generic type array or List<>
  | 'Dict' // Generic type `Dictionary<string, object>`
  | 'Generic' // Other generic types
  | 'Class' // Normal class or struct type
  | 'Def' // Special class for RimWorld Defs
  | 'Unknown'; // Unknown

/**
 * The meta info of the type.
 */
export interface TypeMeta {
  /**
   * The name of the type.
   */
  name: string;
  /**
   * Tye category of the type.
   */
  category: TypeCategory;
  /**
   * The arguments types of list or dict, for other categories this is null.
   */
  of?: TypeMeta;
}
