export * from './download';
import * as download from './download';
export * from './file';
import * as file from './file';
export * from './zip';
import * as zip from './zip';

/**
 * IO for RimTrans
 */
export default {
  ...download,
  ...file,
  ...zip,
};
