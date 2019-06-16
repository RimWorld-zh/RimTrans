export interface Mod {
  /**
   * Path to the directory of the mod
   */
  path: string;

  /**
   * Path to the preview image of the mod.
   */
  previewImage: string;
}

// Constants
export const ID_CORE = 'Core';
export const FOLDER_NAME_ABOUT = 'About';
export const FOLDER_NAME_ASSEMBLIES = 'Assemblies';
export const FOLDER_NAME_DEFS = 'Defs';
export const FOLDER_NAME_PATCHES = 'Patches';
export const FOLDER_NAME_LANGUAGES = 'Languages';
export const FOLDER_NAME_BACKSTORIES = 'Backstories';
export const FOLDER_NAME_DEF_INJECTED = 'DefInjected';
export const FOLDER_NAME_KEYED = 'Keyed';
export const FOLDER_NAME_STRINGS = 'Strings';
