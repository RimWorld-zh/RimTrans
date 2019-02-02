// tslint:disable:no-any no-unsafe-any;

export type LogFunction = (mask: string, ...args: any[]) => void | Promise<void>;

export default interface Logger {
  info: LogFunction;
  success: LogFunction;
  warn: LogFunction;
  error: LogFunction;
}

console;

/**
 * Logger for RimTrans
 */
export default class Logger {}
