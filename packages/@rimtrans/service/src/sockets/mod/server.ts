/**
 * Handler Mods
 */
// tslint:disable:no-any  no-unsafe-any max-func-body-length
import globby from 'globby';
import { genPathResolve } from '@huiji/shared-utils';
import { languageInfos } from '@rimtrans/core';
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
let updating = false;

export const languageCollection: ServerListenerFactory<'languageCollection'> = (
  internal,
  external,
) => {
  const resolveInternal = genPathResolve(internal, 'core', LANGUAGES);
  const resolveExternal = genPathResolve(external, LANGUAGES);

  return async (ws, data) => {
    if (updating) {
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
                status: isInternal && updating ? 'pending' : 'success',
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
      updating = true;
      const timestamp = Date.now();
      const official =
        timestampExternal > timestampInternal
          ? itemsExternal.filter(item => item.internal)
          : itemsInternal;
      official.forEach(item => (item.status = 'pending'));
      const users = itemsExternal.filter(item => !item.internal);

      const send = () =>
        ws.send('languageCollection', {
          timestamp,
          items: [...official, ...users],
        });
      send();

      await Promise.all(
        languageInfos.map(async info => {
          const item = official.find(i => i.name === info.name);
          if (!info.repo || !item) {
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
      updating = false;
    } else if (timestampExternal > timestampInternal) {
      ws.send('languageCollection', {
        timestamp: timestampExternal,
        items: [
          ...itemsExternal.filter(item => item.internal),
          ...itemsExternal.filter(item => !item.internal),
        ],
      });
    } else {
      ws.send('languageCollection', {
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
