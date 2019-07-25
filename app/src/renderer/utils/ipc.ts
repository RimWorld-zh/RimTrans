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
  on<K extends keyof T>(channel: K, listener: IpcListener<T[K]>): void;

  /**
   * Add listener to ipc in specified channel.
   * This is for get message from the main process.
   * Will be remove after called.
   * @param channel the ipc channel
   * @param listener the ipc listener function
   */
  once<K extends keyof T>(channel: K, listener: IpcListener<T[K]>): void;

  /**
   * Send a message to the main process via ipc in specified channel.
   * @param channel the ipc channel
   * @param message the message to send
   */
  send<K extends keyof T>(channel: K, message?: IpcMessage<T[K]>): void;

  /**
   * Remove the ipc listener in specified channel.
   * @param channel the ipc channel
   * @param listener the ipc listener function
   */
  removeListener<K extends keyof T>(channel: K, listener: IpcListener<T[K]>): void;
}

/**
 * Create a IPC interface for the renderer process.
 */
export function createIpc<T extends any = IpcTypeMap>(): IpcRenderer<T> {
  return {
    on(channel, listener) {
      ipcRenderer.on(channel as string, listener);
    },
    once(channel, listener) {
      ipcRenderer.once(channel as string, listener);
    },
    send(channel, message) {
      ipcRenderer.send(channel as string, JSON.parse(JSON.stringify(message)));
    },
    removeListener(channel, listener) {
      ipcRenderer.removeListener(channel as string, listener);
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
    value: createIpc(),
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
