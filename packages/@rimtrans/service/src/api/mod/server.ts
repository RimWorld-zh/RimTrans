/**
 * Handler Mods
 */
// tslint:disable:no-any  no-unsafe-any max-func-body-length
import globby from 'globby';
import { genPathResolve } from '@huiji/shared-utils';
import { languageInfos, LanguageInfo } from '@rimtrans/core';
import io from '@rimtrans/io';

import { ServerListenerFactory, copyModFiles } from '../utils-server';
import {
  ABOUT,
  ABOUT_XML,
  PREVIEW_PNG,
  DEFS,
  LANGUAGES,
  LANGUAGE_INFO_XML,
  FRIENDLY_NAME_TXT,
  LANG_ICON_PNG,
  TIMESTAMP,
  LanguageCollection,
  LanguageData,
} from './model';

const tmp = '.tmp';
let pending = false;

export const coreLanguages: ServerListenerFactory<'coreLanguages'> = (
  internal,
  external,
) => {
  const resolveInternal = genPathResolve(internal, 'core', LANGUAGES);
  const resolveExternal = genPathResolve(external, LANGUAGES);

  return async (wss, data) => {
    if (pending) {
      return;
    }

    const [
      [timestampInternal, itemsInternal],
      [timestampExternal, itemsExternal],
    ] = await Promise.all(
      [resolveInternal, resolveExternal].map<Promise<[number, LanguageData[]]>>(
        async (resolvePath, index) => {
          const pathTimestamp = resolvePath(TIMESTAMP);
          const timestamp = (await io.exists(pathTimestamp))
            ? parseInt(await io.load(pathTimestamp), 10)
            : 0;

          const langs = await globby(['*', `!${tmp}`], {
            cwd: resolvePath('.'),
            onlyDirectories: true,
          });
          langs.sort();

          const items: LanguageData[] = await Promise.all(
            langs.map(async name => {
              const isInternal = languageInfos.some(info => info.name === name);
              const item: LanguageData = {
                name,
                internal: isInternal,
                status: isInternal && pending ? 'pending' : 'success',
              };

              const pathInfo = resolvePath(name, LANGUAGE_INFO_XML);
              const pathFriendly = resolvePath(name, FRIENDLY_NAME_TXT);

              if (await io.exists(pathInfo)) {
                item.info = await io.load(pathInfo, true);
              }
              if (await io.exists(pathFriendly)) {
                item.friendly = (await io.load(pathFriendly)).trim();
              }

              return item;
            }),
          );

          return [timestamp, items];
        },
      ),
    );

    if (data === 'update') {
      pending = true;
      const timestamp = Date.now();

      const users = itemsExternal.filter(item => !item.internal);
      const pairs: [LanguageInfo, LanguageData][] = [];
      const official: LanguageData[] = [];

      languageInfos.forEach(info => {
        const item: LanguageData = (timestampExternal > timestampInternal &&
          itemsExternal.find(i => i.name === info.name)) ||
          itemsInternal.find(i => i.name === info.name) || {
            name: info.name,
            internal: true,
            status: 'pending',
          };
        if (info.repo) {
          item.status = 'pending';
        }
        pairs.push([info, item]);
        official.push(item);
      });

      const send = () => {
        const dataToClient = {
          timestamp,
          items: [...official, ...users],
        };
        wss.send('coreLanguages', dataToClient);
        wss.sendOthers('coreLanguages', dataToClient);
      };
      send();

      await Promise.all(
        pairs.map(async ([info, item]) => {
          if (!info.repo) {
            return;
          }
          try {
            const pathLang = resolveExternal(info.name);
            const pathZip = resolveExternal(tmp, `${info.name}.zip`);
            const pathUnzip = resolveExternal(tmp, `${info.repo}-master`);
            const pathInfo = resolveExternal(info.name, LANGUAGE_INFO_XML);
            const pathFriendly = resolveExternal(info.name, FRIENDLY_NAME_TXT);

            await io.download(
              `https://github.com/Ludeon/${info.repo}/archive/master.zip`,
              pathZip,
              (current, total) => {
                item.current = current;
                item.total = total;
              },
            );
            await io.unzip(pathZip, resolveExternal(tmp));
            await io.remove(pathLang);
            await copyModFiles(pathUnzip, pathLang, [
              LANG_ICON_PNG,
              '**/*.xml',
              '**/*.txt',
              '**/*.md',
            ]);
            if (await io.exists(pathInfo)) {
              item.info = await io.load(pathInfo, true);
            }
            if (await io.exists(pathFriendly)) {
              item.friendly = (await io.load(pathFriendly)).trim();
            }
            item.status = 'success';
            send();
          } catch (error) {
            item.status = 'failed';
            send();
          }
        }),
      );
      await io.save(resolveExternal(TIMESTAMP), timestamp.toString());
      send();
      pending = false;
    } else if (timestampExternal > timestampInternal) {
      wss.send('coreLanguages', {
        timestamp: timestampExternal,
        items: [
          ...itemsExternal.filter(item => item.internal),
          ...itemsExternal.filter(item => !item.internal),
        ],
      });
    } else {
      wss.send('coreLanguages', {
        timestamp: timestampInternal,
        items: [
          ...itemsInternal,
          ...itemsExternal.filter(
            item => !languageInfos.some(info => info.name === item.name),
          ),
        ],
      });
    }
  };
};
