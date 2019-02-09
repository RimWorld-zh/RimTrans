/**
 * Handler Configs
 */

import { genPathResolve } from '@huiji/shared-utils';
import io from '@rimtrans/io';
import { ServerListenerFactory } from '../utils-server';
import { Configs } from './model';

const CONFIGS_JSON = 'configs.json';

export const configs: ServerListenerFactory<'configs'> = (internal, external) => {
  const configsPath = genPathResolve(external)(CONFIGS_JSON);

  const save = async (c: Configs) => {
    try {
      await io.save(configsPath, JSON.stringify(c, undefined, '  '));
    } catch (error) {
      // TODO
    }
  };
  const load = async (): Promise<Configs | undefined> => {
    try {
      const content = await io.load(configsPath);

      return JSON.parse(content) as Configs;
    } catch (error) {
      return undefined;
    }
  };

  return async (wss, data) => {
    if (data) {
      wss.sendOthers('configs', data);
      await save(data);
    } else {
      if (await io.exists(configsPath)) {
        const c = await load();
        wss.send('configs', c);
      } else {
        wss.send('configs', undefined);
      }
    }
  };
};
