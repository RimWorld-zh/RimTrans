import { app } from 'electron';
import { pth, fse, globby } from '@rimtrans/extractor';
import {
  USER_DATA,
  FILE_NAME_SETTINGS,
  FILE_NAME_STORAGE,
  FOLDER_NAME_TRANSLATOR_PROJECTS,
} from '../constants';
import { StateChannel } from './index';

/**
 * The paths record for app.
 * The data directory, execute path and json file paths for states.
 */
export interface Paths extends Record<StateChannel, string> {
  /**
   * The platform-specific file separator. '\\' or '/'.
   */
  readonly separator: '\\' | '/';

  /**
   * The path to the directory for storing the app's configuration files.
   */
  readonly dataDir: string;

  /**
   * The path to the directory for storing translator project files.
   */
  readonly translatorProjects: string;
}

export function createPaths(): Paths {
  const userData = app.getPath(USER_DATA);

  const resolve = (...args: string[]): string => pth.join(userData, ...args);

  const paths: Paths = {
    separator: pth.sep,
    dataDir: userData,
    settings: resolve(FILE_NAME_SETTINGS),
    storage: resolve(FILE_NAME_STORAGE),
    translatorProjects: resolve(FOLDER_NAME_TRANSLATOR_PROJECTS),
  };
  return paths;
}
