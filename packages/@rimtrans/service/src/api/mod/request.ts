import { requestFunction } from '../utils-request';
import { ModsParams } from './model';

/**
 * Request functions for mods
 */
export const mods = {
  /**
   * Requests mods list
   */
  get: requestFunction<[string, string], ModsParams>('get', '/mods'),
};
