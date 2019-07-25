/* eslint-disable @typescript-eslint/no-explicit-any */
import {
  app,
  BrowserWindow,
  ipcMain,
  Event as ElectronEvent,
  WebContents,
  dialog,
} from 'electron';
import * as io from '@rimtrans/io';
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
  settings: Settings;
  storage: Storage;
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
    settings: io.join(userData, FILENAME_SETTINGS),
    storage: io.join(userData, FILENAME_STORAGE),
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
  defaultState: () => StateTypeMap[K];
}

/**
 * Create a `StateWrapper`.
 * @param options the options
 */
function createStateWrapper<K extends StateChannel>(
  ipc: IpcMain<StateTypeMap>,
  options: StateWrapperOptions<K>,
): StateWrapper<StateTypeMap[K]> {
  const { channel, key, path, defaultState } = options;
  type CurrentState = StateTypeMap[K];

  setGlobal<CurrentState>(key, defaultState());

  const save: StateWrapper<CurrentState>['save'] = async () => {
    const state = getGlobal<CurrentState>(key);
    io.save(path, JSON.stringify(state, undefined, '  '));
  };

  const load: StateWrapper<CurrentState>['load'] = async () => {
    if (await io.fileExists(path)) {
      const state = await io.load<Partial<CurrentState>>(path);
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
  paths: Paths;
  browserWindowsSet: Set<BrowserWindow>;
  settings: StateWrapper<Settings>;
  storage: StateWrapper<Storage>;
  loadStates(): Promise<void>;
  saveStates(): Promise<void>;
}

/**
 * Create states collection.
 */
export function createStates(): States {
  const paths = createPaths();
  setGlobal(GLOBAL_KEY_PATHS, paths);

  const browserWindowsSet = new Set<BrowserWindow>();
  const ipc = createIpc<StateTypeMap>(browserWindowsSet);

  const settings = createStateWrapper(ipc, {
    channel: 'settings',
    key: GLOBAL_KEY_SETTINGS,
    path: paths.settings,
    defaultState: defaultSettings,
  });

  const storage = createStateWrapper(ipc, {
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

  return { paths, browserWindowsSet, settings, storage, loadStates, saveStates };
}
