/**
 * Handler Configs
 */
// tslint:disable:no-any  no-unsafe-any

import fs from 'fs';
import pth from 'path';
import express from 'express';
import globby from 'globby';
import { genPathResolve } from '@huiji/shared-utils';
import { languageInfos } from '@rimtrans/core';
import io from '@rimtrans/io';
import { ServerListenerFactory } from '../utils-server';

const CONFIGS_JSON = 'configs.json';

const factory: ServerListenerFactory<'configs' | 'configsInit'> = (
  internal,
  external,
) => {
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

export const configs: ServerListenerFactory<'configs'> = factory;
export const configsInit: ServerListenerFactory<'configsInit'> = factory;
