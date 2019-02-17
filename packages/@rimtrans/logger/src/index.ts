// tslint:disable:no-any no-unsafe-any;

export type LogFunction = (mask: string, ...args: any[]) => void | Promise<void>;

/**
 * The model of logger
 */
export default interface Logger {
  info: LogFunction;
  success: LogFunction;
  warn: LogFunction;
  error: LogFunction;
}
