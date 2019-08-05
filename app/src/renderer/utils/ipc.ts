/* eslint-disable @typescript-eslint/no-explicit-any */
import { remote, Event as ElectronEvent, ipcRenderer } from 'electron';
import Vue, { PluginFunction } from 'vue';
import {
  Component,
  Emit,
  Inject,
  Model,
  Prop,
  Provide,
  Watch,
} from 'vue-property-decorator';
import { IpcTypeMap, IpcMessage, IpcListener } from '@src/main/utils/ipc';

/**
 * The IPC interface for renderer interface.
 */
export interface IpcRenderer<T extends any = IpcTypeMap> {
  /**
   * Add listener to ipc in specified channel.
   * This is for get message from the main process.
   * @param channel the ipc channel
   * @param listener the ipc listener function
   */
  on<K extends keyof T>(channel: K, listener: IpcListener<T[K][0]>): void;

  /**
   * Add listener to ipc in specified channel.
   * This is for get message from the main process.
   * Will be remove after called.
   * @param channel the ipc channel
   * @param listener the ipc listener function
   */
  once<K extends keyof T>(channel: K, listener: IpcListener<T[K][0]>): void;

  /**
   * Send a message to the main process via ipc in specified channel.
   * @param channel the ipc channel
   * @param message the message to send
   */
  send<K extends keyof T>(channel: K, message?: IpcMessage<T[K][0]>): void;

  /**
   * Remove the ipc listener in specified channel.
   * @param channel the ipc channel
   * @param listener the ipc listener function
   */
  removeListener<K extends keyof T>(channel: K, listener: IpcListener<T[K][0]>): void;

  request<K extends keyof T>(channel: K, data: T[K][0]): Promise<T[K][1]>;
}

function generateID(): number {
  return Math.floor((Date.now() + Math.random()) * 1000);
}

/**
 * Create a IPC interface for the renderer process.
 */
export function createIpc<T extends any = IpcTypeMap>(namespace: string): IpcRenderer<T> {
  return {
    on(channel, listener) {
      ipcRenderer.on(`${namespace}-${channel}`, listener);
    },
    once(channel, listener) {
      ipcRenderer.once(`${namespace}-${channel}`, listener);
    },
    send(channel, message) {
      ipcRenderer.send(`${namespace}-${channel}`, JSON.parse(JSON.stringify(message)));
    },
    removeListener(channel, listener) {
      ipcRenderer.removeListener(`${namespace}-${channel}`, listener);
    },

    request<K extends keyof T>(channel: K, data: T[K][0]): Promise<T[K][1]> {
      return new Promise<T[K][1]>((resolve, reject) => {
        const realChannel = `${namespace}-${channel}`;
        const id = generateID();
        const message = { id, data };

        const listener = (
          event: ElectronEvent,
          replyMessage: IpcMessage<T[K][1]>,
          error?: Error,
        ): void => {
          if (replyMessage.id !== id) {
            return;
          }
          ipcRenderer.removeListener(realChannel, listener);
          if (error) {
            reject(error);
            return;
          }
          resolve(replyMessage.data);
        };

        ipcRenderer.on(realChannel, listener);
        ipcRenderer.send(realChannel, message);
      });
    },
  };
}

declare module 'vue/types/vue' {
  interface Vue {
    readonly $ipc: IpcRenderer;
  }
}

let $$Vue: typeof Vue | undefined;

export const PluginIpc: PluginFunction<never> = $Vue => {
  if ($$Vue && $$Vue === $Vue) {
    return;
  }
  $$Vue = $Vue;

  Object.defineProperty($Vue.prototype, '$ipc', {
    configurable: false,
    writable: false,
    enumerable: false,
    value: createIpc('app'),
  });
};

// ------------------------------------------------
// Global

/**
 * Get global value of the main process.
 * @param key the name of the global value.
 */
export function getGlobal<T>(key: string): T {
  return JSON.parse(JSON.stringify(remote.getGlobal(key)));
}
