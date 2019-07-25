/* eslint-disable @typescript-eslint/no-explicit-any */
import {
  dialog,
  BrowserWindow,
  Event as ElectronEvent,
  WebContents,
  ipcMain,
} from 'electron';

export interface IpcTypeMap {
  test: string;
}

export type IpcChannel = keyof IpcTypeMap;

export interface IpcMessage<T> {
  /**
   * The ID of the `BrowserWindow`
   */
  id?: number;
  data: T;
}

export type IpcListener<T> = (event: ElectronEvent, message?: IpcMessage<T>) => any;

/**
 * The IPC interface for main process.
 */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export interface IpcMain<T extends any = IpcTypeMap> {
  /**
   * Add listener to ipc in specified channel.
   * This is for get message from renderer processes.
   * @param channel the ipc channel
   * @param listener the ipc listener function
   */
  on<K extends keyof T>(channel: K, listener: IpcListener<T[K]>): void;

  /**
   * Add listener to ipc in specified channel.
   * This is for get message from renderer processes.
   * Will be remove after called.
   * @param channel the ipc channel
   * @param listener the ipc listener function
   */
  once<K extends keyof T>(channel: K, listener: IpcListener<T[K]>): void;

  /**
   * Send a message to a renderer process via ipc in specified channel.
   * @param webContents the `WebContents` of the `BrowserWindow`, or event sender for reply.
   * @param channel the ipc channel
   * @param message the message to send
   */
  send<K extends keyof T>(
    webContents: WebContents,
    channel: K,
    message?: IpcMessage<T[K]>,
  ): void;

  /**
   * Send a message to all renderer processes via ipc in specified channel.
   * @param channel the ipc channel
   * @param message the message to send
   */
  sendAll<K extends keyof T>(channel: K, message?: IpcMessage<T[K]>): void;

  /**
   * Remove the ipc listener in specified channel.
   * @param channel the ipc channel
   * @param listener the ipc listener function
   */
  removeListener<K extends keyof T>(channel: K, listener: IpcListener<T[K]>): void;
}

/**
 * Create a IPC interface from the main process.
 * @param browserWindowsSet the `Set` of `BrowserWindow`
 */
export function createIpc<T extends any = IpcTypeMap>(
  browserWindowsSet: Set<BrowserWindow>,
): IpcMain<T> {
  return {
    on(channel, listener) {
      ipcMain.on(channel as string, listener);
    },
    once(channel, listener) {
      ipcMain.once(channel as string, listener);
    },
    send(webContents, channel, message) {
      webContents.send(channel as string, JSON.parse(JSON.stringify(message)));
    },
    sendAll(channel, message) {
      browserWindowsSet.forEach((win: BrowserWindow) =>
        win.webContents.send(channel as string, JSON.parse(JSON.stringify(message))),
      );
    },
    removeListener(channel, listener) {
      ipcMain.removeListener(channel as string, listener);
    },
  };
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
