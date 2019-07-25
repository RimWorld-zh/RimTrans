export interface Settings {
  /**
   * The interface language for app.
   */
  language: string;

  /**
   * The path to the directory of RimWorld installation.
   */
  directoryRimWorld: string;
  /**
   * The path to the directory of RimWorld workshop mods.
   */
  directoryWorkshop: string;
}

export function defaultSettings(): Settings {
  return {
    language: 'auto',

    directoryRimWorld: '',
    directoryWorkshop: '',
  };
}
