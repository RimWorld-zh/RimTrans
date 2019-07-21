/* eslint-disable @typescript-eslint/no-explicit-any */
import {
  app,
  BrowserWindow,
  ipcMain,
  Event as ElectronEvent,
  WebContents,
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
import { Settings, defaultSettings } from './settings';
import { Storage, defaultStorage } from './storage';

// ------------------------------------------------
// Models

/**
 * The type map for ipc channels and states.
 * **NOTE** the state must be a object.
 */
export interface ChannelStateMap {
  settings: Settings;
  storage: Storage;
}

/**
 * The ipc channels for states.
 */
export type Channel = keyof ChannelStateMap;

/**
 * The type map for ipc channels and various types (includes states).
 */
export interface IpcChannelTypeMap extends ChannelStateMap {
  test: undefined;
}

/**
 * All ipc channels.
 */
export type IpcChannel = keyof IpcChannelTypeMap;

/**
 * The wrapper for ipc message.
 */
export interface IpcMessage<T extends IpcChannel> {
  /**
   * The ID of the `BrowserWindow`
   */
  id?: number;
  data: IpcChannelTypeMap[T];
}

/**
 * The type for ipc listener function.
 */
export type IpcListener<T extends IpcChannel> = (
  event: ElectronEvent,
  message: IpcMessage<T>,
) => any;

/**
 * The paths record for app.
 * The data directory, execute path and json file paths for states.
 */
export interface Paths extends Record<Channel, string> {
  readonly dataDir: string;
}

// ------------------------------------------------
// Global

/**
 * Get global value.
 * @param key the name of the global value.
 */
export function getGlobal<T>(key: string): T {
  return (global as any)[key];
}

/**
 * Set global value.
 * @param key the name of the global value.
 * @param value the value
 */
export function setGlobal<T>(key: string, value: T): void {
  (global as any)[key] = value;
}

// ------------------------------------------------
// IPC

/**
 * Add listener to ipc in specified channel.
 * This is for get message from web contents.
 * @param channel the ipc channel
 * @param listener the ipc listener function
 */
export function ipcOn<T extends IpcChannel>(channel: T, listener: IpcListener<T>): void {
  ipcMain.on(channel, listener);
}

/**
 * Add listener to ipc in specified channel. Will be remove after called.
 * This is for get message from web contents.
 * @param channel the ipc channel
 * @param listener the ipc listener function
 */
export function ipcOnce<T extends IpcChannel>(
  channel: T,
  listener: IpcListener<T>,
): void {
  ipcMain.once(channel, listener);
}

/**
 * Send a message via ipc in specified channel to web contents.
 * @param webContents the `WebContents` of the `BrowserWindow`, or event sender for reply.
 * @param channel the ipc channel
 * @param message the message to send
 */
export function ipcSend<T extends IpcChannel>(
  webContents: WebContents,
  channel: T,
  message: IpcMessage<T>,
): void {
  webContents.send(channel, message);
}

/**
 * Send a message via ipc in specified channel to a set of `BrowserWindow`.
 * @param browserWindowsSet the `Set` of `BrowserWindow`
 * @param channel the ipc channel
 * @param message the message to send
 */
export function ipcSendAll<T extends IpcChannel>(
  browserWindowsSet: Set<BrowserWindow> | BrowserWindow[],
  channel: T,
  message: IpcMessage<T>,
): void {
  browserWindowsSet.forEach((win: BrowserWindow) =>
    win.webContents.send(channel, message),
  );
}

/**
 * Remove the ipc listener in specified channel
 * @param channel the ipc channel
 * @param listener the ipc listener function
 */
export function ipcRemoveListener<T extends IpcChannel>(
  channel: T,
  listener: IpcListener<T>,
): void {
  ipcMain.removeListener(channel, listener);
}

// ------------------------------------------------
// State

/**
 * The state wrapper interface.
 */
export interface StateWrapper<T extends Channel> {
  get(): ChannelStateMap[T];
  set(partial: Partial<ChannelStateMap[T]>, emit?: boolean): void;
  load(): Promise<void>;
  save(): Promise<void>;
}

export interface StateWrapperOptions<T extends Channel> {
  /**
   * the ipc channel
   */
  channel: T;
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
  defaultState: () => ChannelStateMap[T];
}

/**
 * Create a `StateWrapper`.
 * @param browserWindowsSet the `Set` of `BrowserWindow`
 * @param options the options
 */
export function createStateWrapper<T extends Channel>(
  browserWindowsSet: Set<BrowserWindow>,
  options: StateWrapperOptions<T>,
): StateWrapper<T> {
  const { channel, key, path, defaultState } = options;
  type CurrentState = ChannelStateMap[T];

  setGlobal<CurrentState>(key, defaultState());

  const save: StateWrapper<T>['save'] = async () => {
    const state = getGlobal<CurrentState>(key);
    io.save(path, JSON.stringify(state, undefined, '  '));
  };

  const load: StateWrapper<T>['load'] = async () => {
    if (await io.fileExists(path)) {
      const state = await io.load<CurrentState>(path);
      setGlobal<CurrentState>(key, state);
    } else {
      await save();
    }
  };

  const set: StateWrapper<T>['set'] = (partial, emit = true) => {
    const state: CurrentState = {
      ...getGlobal<CurrentState>(key),
      ...partial,
    };
    setGlobal<CurrentState>(key, state);
    save();
    if (emit) {
      ipcSendAll(browserWindowsSet, channel, { data: state as IpcChannelTypeMap[T] });
    }
  };

  const get: StateWrapper<T>['get'] = () => getGlobal(key);

  ipcOn(channel, (event, { data }) => set(data, true));

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
  settings: StateWrapper<'settings'>;
  storage: StateWrapper<'storage'>;
  initStates(): Promise<void>;
}

/**
 * Create states collection.
 */
export function createStates(): States {
  const userData = app.getPath(USER_DATA);
  const paths: Paths = {
    dataDir: userData,
    settings: io.join(userData, FILENAME_SETTINGS),
    storage: io.join(userData, FILENAME_STORAGE),
  };
  setGlobal(GLOBAL_KEY_PATHS, paths);

  const browserWindowsSet = new Set<BrowserWindow>();

  const settings = createStateWrapper(browserWindowsSet, {
    channel: 'settings',
    key: GLOBAL_KEY_SETTINGS,
    path: paths.settings,
    defaultState: defaultSettings,
  });

  const storage = createStateWrapper(browserWindowsSet, {
    channel: 'storage',
    key: GLOBAL_KEY_STORAGE,
    path: paths.storage,
    defaultState: defaultStorage,
  });

  const initStates = async (): Promise<void> => {
    await Promise.all([settings, storage].map(state => state.load()));
  };

  return { paths, browserWindowsSet, settings, storage, initStates };
}
