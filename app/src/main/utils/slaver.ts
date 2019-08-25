/* eslint-disable @typescript-eslint/no-explicit-any */
import cp, { ChildProcess } from 'child_process';

export interface SlaverTypeMap {
  foobar: [
    { foo: string }, // from main to sub process
    { bar: string }, // from sub to main process
  ];
}

export type SlaverListener<T> = (data: T) => any;

/**
 * The slaver interface for main process.
 */
export interface SlaverMain<T extends any = SlaverTypeMap> {
  readonly childProcess: ChildProcess;

  /**
   * Send data to sub process in specified action.
   * @param action the action
   * @param data the data
   */
  send<K extends keyof T>(action: K, data: T[K][0]): void;

  /**
   * Listen specified action for resolving data from sub process.
   * @param action
   * @param listener
   */
  addListener<K extends keyof T>(action: K, listener: SlaverListener<T[K][1]>): void;
}

/**
 * The slaver interface for sub process.
 */
export interface SlaverSub<T extends any = SlaverTypeMap> {
  /**
   * Send data to main process in specified action.
   * @param action the action
   * @param data the data
   */
  send<K extends keyof T>(action: K, data: T[K][1]): void;

  /**
   * Listen specified action for resolving data from main process.
   * @param action
   * @param listener
   */
  addListener<K extends keyof T>(action: K, listener: SlaverListener<T[K][0]>): void;
}

export function createSlaverMain<T extends any = SlaverTypeMap>(
  path: string,
  args?: readonly string[],
): SlaverMain<T> {
  const childProcess = cp.fork(path, args, {
    silent: true,
    detached: true,
    // stdio: 'ignore',
    env: {
      ELECTRON_RUN_AS_NODE: '1',
    },
  });

  const listenerMap: { [K in keyof T]?: SlaverListener<T[K][1]>[] } = {};

  childProcess.on('message', ([action, data]) => {
    const listeners = listenerMap[action];
    if (listeners) {
      listeners.forEach(l => l(data));
    }
  });

  const send: SlaverMain<T>['send'] = (action, data) => childProcess.send([action, data]);

  const addListener: SlaverMain<T>['addListener'] = (action, listener) => {
    const listeners = listenerMap[action] || (listenerMap[action] = []);
    (listeners as Function[]).push(listener);
  };

  return { childProcess, send, addListener };
}

type Send = (
  message: any,
  sendHandle?: any,
  options?: { swallowErrors?: boolean },
  callback?: (error: Error | null) => void,
) => boolean;

export function createSlaverSub<T extends any = SlaverTypeMap>(): SlaverSub<T> {
  if (!process.send) {
    throw new Error('createSlaverSub must be called in sub process.');
  }

  const listenerMap: { [K in keyof T]?: SlaverListener<T[K][1]>[] } = {};

  process.on('message', ([action, data]) => {
    const listeners = listenerMap[action];
    if (listeners) {
      listeners.forEach(l => l(data));
    }
  });

  const send: SlaverSub<T>['send'] = (action, data) =>
    (process.send as Send)([action, data]);

  const addListener: SlaverSub<T>['addListener'] = (action, listener) => {
    const listeners = listenerMap[action] || (listenerMap[action] = []);
    (listeners as Function[]).push(listener);
  };

  return { send, addListener };
}
