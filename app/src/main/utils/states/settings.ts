import { systemPreferences } from 'electron';
import { Theme } from '../../../renderer/components/base/variables';

export interface Settings {
  theme: Theme;

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
    theme: systemPreferences.isDarkMode() ? 'dark' : 'light',
    language: 'auto',

    directoryRimWorld: '',
    directoryWorkshop: '',
  };
}
