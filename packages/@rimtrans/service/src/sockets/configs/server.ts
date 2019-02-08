/**
 * Handler Configs
 */
// tslint:disable:no-any  no-unsafe-any

import { genPathResolve } from '@huiji/shared-utils';
import io from '@rimtrans/io';
import { ServerListenerFactory } from '../utils-server';

const CONFIGS_JSON = 'configs.json';

export const configs: ServerListenerFactory<'configs'> = (internal, external) => {
  const configsPath = genPathResolve(external)(CONFIGS_JSON);

  const save = async (c: any) => {
    try {
      await io.save(configsPath, JSON.stringify(c, undefined, '  '));
    } catch (error) {
      // TODO
    }
  };
  const load = async (): Promise<any> => {
    try {
      const content = await io.load(configsPath);

      return JSON.parse(content);
    } catch (error) {
      return undefined;
    }
  };

  return async (ws, data) => {
    if (data) {
      await save(data);
    } else {
      if (await io.exists(configsPath)) {
        const c = await load();
        ws.send('configs', c);
      } else {
        ws.send('configs', undefined);
      }
    }
  };
};
