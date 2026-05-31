/* eslint-disable @typescript-eslint/no-explicit-any */
import { dialog, BrowserWindow, IpcMainEvent, WebContents, ipcMain } from 'electron';
import { cloneObject } from './object';
import { StateTypeMap } from './states';

export interface IpcTypeMap {
  // must use tuple type
  foobar: [
    { foo: string }, // type from renderer process to main process.
    { bar: string }, // type from main process to renderer process.
  ];
}

export type IpcChannel = keyof IpcTypeMap;

export interface IpcMessage<T> {
  /**
   * The ID of the `BrowserWindow` or a request
   */
  id?: number | string;
  data: T;
}

export type IpcMainListener<T> = (event: IpcMainEvent, message: IpcMessage<T>) => any;

export type IpcMainHandler<T, R> = (event: IpcMainEvent, data: T) => Promise<R>;

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
  on<K extends keyof T>(channel: K, listener: IpcMainListener<T[K][0]>): void;

  /**
   * Add listener to ipc in specified channel.
   * This is for get message from renderer processes.
   * Will be remove after called.
   * @param channel the ipc channel
   * @param listener the ipc listener function
   */
  once<K extends keyof T>(channel: K, listener: IpcMainListener<T[K][0]>): void;

  /**
   * Send a message to a renderer process via ipc in specified channel.
   * @param webContents the `WebContents` of the `BrowserWindow`, or event sender for reply.
   * @param channel the ipc channel
   * @param message the message to send
   */
  send<K extends keyof T>(
    webContents: WebContents,
    channel: K,
    message: IpcMessage<T[K][1]>,
  ): void;

  /**
   * Send a message to all renderer processes via ipc in specified channel.
   * @param channel the ipc channel
   * @param message the message to send
   */
  sendAll<K extends keyof T>(channel: K, message: IpcMessage<T[K][1]>): void;

  /**
   * Remove the ipc listener in specified channel.
   * @param channel the ipc channel
   * @param listener the ipc listener function
   */
  removeListener<K extends keyof T>(channel: K, listener: IpcMainListener<T[K][0]>): void;

  /**
   * Remove all of listeners in specified channels.
   * @param channels the ipc channels
   */
  removeAllListeners<K extends keyof T>(...channels: K[]): void;

  /**
   * Add handler to response request from renderer process in specified channel.
   * @param channel the ipc channel
   * @param handler the handler function for reply value to the renderer process request
   */
  addRequestHandler<K extends keyof T>(
    channel: K,
    handler: IpcMainHandler<T[K][0], T[K][1]>,
  ): void;

  /**
   * Remove handler in specified channel.
   * @param channel the ipc channel
   * @param handler the handler
   */
  removeRequestHandler<K extends keyof T>(
    channel: K,
    handler: IpcMainHandler<T[K][0], T[K][1]>,
  ): void;

  /**
   * Checks listeners, for debugging.
   * @param eventRequiredRecord the map to signing requirement for events
   */
  checkListeners(eventRequiredRecord: Record<keyof T, boolean>): boolean;
}

/**
 * Create a IPC interface from the main process.
 * @param browserWindowsSet the `Set` of `BrowserWindow`
 */
export function createIpc<T extends any = IpcTypeMap>(namespace: string): IpcMain<T> {
  if (!namespace) {
    throw new Error("Must provide the argument 'namespace'");
  }

  const handlerListenerMap = new Map<Function, Function>();

  return {
    on(channel, listener) {
      ipcMain.on(`${namespace}-${channel}`, listener);
    },
    once(channel, listener) {
      ipcMain.once(`${namespace}-${channel}`, listener);
    },

    send(webContents, channel, message) {
      webContents.send(`${namespace}-${channel}`, cloneObject(message));
    },
    sendAll(channel, message) {
      BrowserWindow.getAllWindows().forEach((win: BrowserWindow) =>
        win.webContents.send(`${namespace}-${channel}`, cloneObject(message)),
      );
    },

    removeListener(channel, listener) {
      ipcMain.removeListener(`${namespace}-${channel}`, listener);
    },
    removeAllListeners(...channels) {
      const entries = Array.from(handlerListenerMap.entries());
      channels.forEach(channel => {
        const realChannel = `${namespace}-${channel}`;
        ipcMain.listeners(realChannel).forEach(listener => {
          const pair = entries.find(([k, v]) => v === listener);
          if (pair) {
            handlerListenerMap.delete(pair[0]);
          }
        });
        ipcMain.removeAllListeners(realChannel);
      });
    },

    addRequestHandler<K extends keyof T>(
      channel: K,
      handler: IpcMainHandler<T[K][0], T[K][1]>,
    ): void {
      if (handlerListenerMap.has(handler)) {
        return;
      }
      const realChannel = `${namespace}-${channel}`;

      const listener = async (
        event: IpcMainEvent,
        message: IpcMessage<T[K][0]>,
      ): Promise<void> => {
        const { id, data } = message;
        const replyData = await handler(event, data);
        const replyMessage: IpcMessage<T[K][1]> = {
          id,
          data: replyData,
        };
        event.sender.send(realChannel, cloneObject(replyMessage));
      };

      handlerListenerMap.set(handler, listener);
      ipcMain.on(realChannel, listener);
    },

    removeRequestHandler<K extends keyof T>(
      channel: K,
      handler: IpcMainHandler<T[K][0], T[K][1]>,
    ): void {
      if (handlerListenerMap.has(handler)) {
        const listener = handlerListenerMap.get(handler) as IpcMainListener<T[K][0]>;
        handlerListenerMap.delete(handler);
        ipcMain.removeListener(`${namespace}-${channel}`, listener);
      }
    },

    checkListeners(eventRequiredRecord: Record<keyof T, boolean>): boolean {
      const existsPairs = Object.entries(eventRequiredRecord).map<[string, boolean]>(
        ([channel, required]) => {
          if (!required) {
            return [channel, true];
          }
          const listeners = ipcMain.listeners(`${namespace}-${channel}`);
          return [channel, listeners.length > 0];
        },
      );
      const existsMap = Object.fromEntries(existsPairs);

      const result = existsPairs.reduce<boolean>(
        (sum, [, exists]) => sum && exists,
        true,
      );

      if (process.env.NODE_ENV === 'development' && !result) {
        dialog.showErrorBox(
          `Missing listener for ipc namespace '${namespace}'`,
          JSON.stringify(existsMap, undefined, '  '),
        );
      }

      return result;
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
