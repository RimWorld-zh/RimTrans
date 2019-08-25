import { app } from 'electron';
import { pth, fse, globby } from '@rimtrans/extractor';
import {
  USER_DATA,
  FILENAME_SETTINGS,
  FILENAME_STORAGE,
  FOLDER_NAME_TRANSLATOR_PROJECTS,
} from '../constants';
import { StateChannel } from './index';

/**
 * The paths record for app.
 * The data directory, execute path and json file paths for states.
 */
export interface Paths extends Record<StateChannel, string> {
  readonly dataDir: string;
  readonly translatorProjects: string;
}

export function createPaths(): Paths {
  const userData = app.getPath(USER_DATA);

  const resolve = (...args: string[]): string => pth.join(userData, ...args);

  const paths: Paths = {
    dataDir: userData,
    settings: resolve(FILENAME_SETTINGS),
    storage: resolve(FILENAME_STORAGE),
    translatorProjects: resolve(FOLDER_NAME_TRANSLATOR_PROJECTS),
  };
  return paths;
}
