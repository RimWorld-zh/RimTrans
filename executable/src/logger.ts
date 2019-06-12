/* eslint-disable no-console,@typescript-eslint/no-explicit-any */
import chalk from 'chalk';
import timestamp from 'time-stamp';

export type Log = (message?: any, ...optionalParams: any[]) => void;

export const info: Log = (message, ...optionalParams) => {
  console.log(
    timestamp('YYYY-MM-DD HH:mm:ss.ms'),
    chalk.cyanBright('INFO'),
    message,
    ...optionalParams,
  );
};

export const success: Log = (message, ...optionalParams) => {
  console.log(
    new Date().toLocaleString(),
    chalk.greenBright('SUCCESS'),
    message,
    ...optionalParams,
  );
};

export const warn: Log = (message, ...optionalParams) => {
  console.warn(
    new Date().toLocaleString(),
    chalk.yellowBright('WARN'),
    message,
    ...optionalParams,
  );
};

export const error: Log = (message, ...optionalParams) => {
  console.error(
    new Date().toLocaleString(),
    chalk.redBright('ERROR'),
    message,
    ...optionalParams,
  );
};
