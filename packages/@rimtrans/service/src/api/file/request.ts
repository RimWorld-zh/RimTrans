// tslint:disable:
import { BASE_URL_REST_API } from '../model';
import { requestFunction } from '../utils-request';
import { FileParams } from './model';

/**
 * Request function for files
 */
export const file = {
  /**
   * Get the full url for requesting a file.
   * @param path the path to the file
   */
  url(path: string): string {
    return `${BASE_URL_REST_API}/file?path=${path}`;
  },
  get: requestFunction<string, FileParams>('get', '/file'),
};
