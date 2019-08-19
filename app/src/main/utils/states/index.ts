/* eslint-disable @typescript-eslint/no-explicit-any */
import {
  app,
  BrowserWindow,
  ipcMain,
  Event as ElectronEvent,
  WebContents,
  dialog,
} from 'electron';
import { pth, fse, globby } from '@rimtrans/extractor';
import {
  USER_DATA,
  GLOBAL_KEY_PATHS,
  GLOBAL_KEY_SETTINGS,
  FILENAME_SETTINGS,
  GLOBAL_KEY_STORAGE,
  FILENAME_STORAGE,
} from '../constants';
import { IpcMain, createIpc, getGlobal, setGlobal } from '../ipc';
import { Settings, defaultSettings } from './settings';
import { Storage, defaultStorage } from './storage';

export interface StateTypeMap {
  settings: [Settings];
  storage: [Storage];
}

export type StateChannel = keyof StateTypeMap;

// ------------------------------------------------
// Paths

/**
 * The paths record for app.
 * The data directory, execute path and json file paths for states.
 */
export interface Paths extends Record<StateChannel, string> {
  readonly dataDir: string;
}

function createPaths(): Paths {
  const userData = app.getPath(USER_DATA);
  const paths: Paths = {
    dataDir: userData,
    settings: pth.join(userData, FILENAME_SETTINGS),
    storage: pth.join(userData, FILENAME_STORAGE),
  };
  return paths;
}

// ------------------------------------------------
// State

/**
 * The state wrapper interface.
 */
export interface StateWrapper<T> {
  get(): T;
  set(partial: Partial<T>, emit?: boolean, id?: number): void;
  load(): Promise<void>;
  save(): Promise<void>;
}

export interface StateWrapperOptions<K extends StateChannel> {
  /**
   * the ipc channel
   */
  channel: K;
  /**
   * the name of global value
   */
  key: string;
  /**
   * the path to the file to save state
   */
  path: string;
  /**
   * the factory function for create default state
   */
  defaultState: () => StateTypeMap[K][0];
}

/**
 * Create a `StateWrapper`.
 * @param options the options
 */
function createStateWrapper<K extends StateChannel>(
  ipc: IpcMain<StateTypeMap>,
  options: StateWrapperOptions<K>,
): StateWrapper<StateTypeMap[K][0]> {
  const { channel, key, path, defaultState } = options;
  type CurrentState = StateTypeMap[K][0];

  setGlobal<CurrentState>(key, defaultState());

  const save: StateWrapper<CurrentState>['save'] = async () => {
    const state = getGlobal<CurrentState>(key);
    fse.outputJSON(path, state, { spaces: 2 });
  };

  const load: StateWrapper<CurrentState>['load'] = async () => {
    if (await fse.pathExists(path)) {
      const state = await fse.readJSON(path);
      setGlobal<CurrentState>(key, {
        ...defaultState(),
        ...state,
      });
    } else {
      await save();
    }
  };

  const set: StateWrapper<CurrentState>['set'] = (partial, emit = true, id?) => {
    const state: CurrentState = {
      ...getGlobal<CurrentState>(key),
      ...partial,
    };
    setGlobal<CurrentState>(key, state);
    save();
    if (emit) {
      ipc.sendAll(channel, { id, data: state });
    }
  };

  const get: StateWrapper<CurrentState>['get'] = () => getGlobal(key);

  ipc.on(channel, (event, message) => {
    if (message) {
      set(message.data, true, event.sender.id);
    }
  });

  return {
    get,
    set,
    load,
    save,
  };
}

/**
 * The states collection.
 */
export interface States {
  /**
   * Environment Paths
   */
  paths: Paths;

  /**
   * The `Set` for all `BrowserWindow`
   */
  browserWindowsSet: Set<BrowserWindow>;

  /**
   * Default App IPC
   */
  ipc: IpcMain;

  /**
   * State Settings
   */
  settings: StateWrapper<Settings>;

  /**
   * State Storage
   */
  storage: StateWrapper<Storage>;

  /**
   * Load all states
   */
  loadStates(): Promise<void>;

  /**
   * Save all states
   */
  saveStates(): Promise<void>;
}

/**
 * Create states collection.
 */
export function createStates(): States {
  const paths = createPaths();
  setGlobal(GLOBAL_KEY_PATHS, paths);

  const browserWindowsSet = new Set<BrowserWindow>();
  const ipc = createIpc(browserWindowsSet, 'app');
  const ipcStates = createIpc<StateTypeMap>(browserWindowsSet, 'states');

  const settings = createStateWrapper(ipcStates, {
    channel: 'settings',
    key: GLOBAL_KEY_SETTINGS,
    path: paths.settings,
    defaultState: defaultSettings,
  });

  const storage = createStateWrapper(ipcStates, {
    channel: 'storage',
    key: GLOBAL_KEY_STORAGE,
    path: paths.storage,
    defaultState: defaultStorage,
  });

  const states = [settings, storage];

  const loadStates = async (): Promise<void> => {
    await Promise.all(states.map(state => state.load()));
  };
  const saveStates = async (): Promise<void> => {
    await Promise.all(states.map(state => state.save()));
  };

  return { paths, browserWindowsSet, ipc, settings, storage, loadStates, saveStates };
}
