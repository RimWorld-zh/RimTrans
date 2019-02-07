/**
 * Handler Core
 */
// tslint:disable:max-func-body-length

import fs from 'fs';
import pth from 'path';
import express from 'express';
import globby from 'globby';
import { genPathResolve } from '@huiji/shared-utils';
import { languageInfos, LanguageManifest } from '@rimtrans/core';
import io, { load } from '@rimtrans/io';
import { setContentType } from '../utils-handlers';
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
  Languages,
  LanguageData,
} from './models';

async function copy(src: string, dest: string, patterns: string[]): Promise<void> {
  const resolveSrc = genPathResolve(src);
  const resolveDest = genPathResolve(dest);

  const files = await globby(patterns, { cwd: src });

  files.sort();

  await Promise.all(
    [...new Set(files.map(f => pth.dirname(f)))].map(async dir =>
      io.createDirectory(resolveDest(dir)),
    ),
  );

  await Promise.all(
    files.map(async f => {
      if (/\.(md|xml|txt)$/.test(f)) {
        await io.save(resolveDest(f), await io.load(resolveSrc(f), true));
      } else {
        await io.copy(resolveSrc(f), resolveDest(f));
      }
    }),
  );

  await io.save(
    resolveDest('manifest.json'),
    JSON.stringify(
      {
        files,
      },
      undefined,
      '  ',
    ),
  );
}

/**
 * Generate a router handler for Core
 * @param internal the path to the internal directory for Core
 * @param external the path to the external directory for Core
 */
export function handlerCore(internal: string, external: string): express.Router {
  const router = express.Router();

  const resolveInternal = genPathResolve(internal);
  const resolveExternal = genPathResolve(external);

  let updating = false;
  const updatedCallbacks: Function[] = [];

  router
    .get('/about', async (request, response) => {
      setContentType(response, 'text/xml').sendFile(resolveInternal(ABOUT, ABOUT_XML));
    })
    .get('/preview', async (request, response) => {
      const path = resolveInternal(ABOUT, PREVIEW_PNG);
      if (fs.existsSync(path)) {
        response.sendFile(path);
      } else {
        response.sendStatus(404);
      }
    })
    .get('/defs', async (request, response) => {
      const fileMap: Record<string, string> = {};
      const filePaths = await globby('**/*.xml', { cwd: resolveInternal(DEFS) });
      await Promise.all(
        filePaths.map(async f => (fileMap[f] = await io.load(resolveInternal(DEFS, f)))),
      );
      response.send(fileMap);
    })
    .get('/languages', async (request, response) => {
      const languages: Languages = {
        timestamp: -1,
        items: [],
      };
      languageInfos.forEach(info =>
        languages.items.push({
          name: info.name,
        }),
      );

      for (const resolvePath of [resolveInternal, resolveExternal]) {
        const dirs = await globby('*', {
          cwd: resolvePath(LANGUAGES),
          onlyDirectories: true,
        });
        dirs.sort();

        const items = await Promise.all(
          dirs.map<Promise<LanguageData>>(async name => {
            try {
              const info = resolvePath(LANGUAGES, name, LANGUAGE_INFO_XML);
              const friendly = resolvePath(LANGUAGES, name, FRIENDLY_NAME_TXT);
              if (fs.existsSync(info)) {
                return {
                  name,
                  info: await io.load(info, true),
                };
              } else if (fs.existsSync(friendly)) {
                return {
                  name,
                  friendly: (await io.load(friendly)).trim(),
                };
              } else {
                return {
                  name,
                };
              }
            } catch (error) {
              return {
                name,
              };
            }
          }),
        );

        let timestamp: number;
        const timestampPath = resolvePath(LANGUAGES, TIMESTAMP);
        if (fs.existsSync(timestampPath)) {
          try {
            timestamp = parseInt((await io.load(timestampPath)).trim(), 10);
          } catch (error) {
            timestamp = 0;
          }
        } else {
          timestamp = 0;
        }

        items.forEach(item => {
          const index = languages.items.findIndex(i => i.name === item.name);
          if (index > -1) {
            if (timestamp > languages.timestamp) {
              languages.items[index] = item;
            }
          } else {
            languages.items.push(item);
          }
        });

        if (timestamp > languages.timestamp) {
          languages.timestamp = timestamp;
        }
      }

      response.send(languages);
    })
    .get('/languages-update-all', async (request, response) => {
      if (updating) {
        updatedCallbacks.push(() => response.sendStatus(200));
        console.log('roll back');

        return;
      }
      updating = true;

      const tmp = '.tmp-languages';
      await io.remove(resolveExternal(tmp));

      const timestamp = Date.now();
      for (const info of languageInfos) {
        if (!info.repo) {
          continue;
        }
        const url = `https://github.com/Ludeon/${info.repo}/archive/master.zip`;
        const zip = resolveExternal(tmp, `${info.name}.zip`);
        await io.download(url, zip);
        await io.unzip(zip, resolveExternal(tmp));
        await io.remove(resolveExternal(LANGUAGES, info.name));
        await copy(
          resolveExternal(tmp, `${info.repo}-master`),
          resolveExternal(LANGUAGES, info.name),
          [LANG_ICON_PNG, '**/*.xml', '**/*.txt', '**/*.md'],
        );
      }
      await io.save(resolveExternal(LANGUAGES, TIMESTAMP), timestamp.toString());

      updating = false;
      updatedCallbacks.splice(0).forEach(cb => cb());
      response.sendStatus(200);
    })
    .get('/language/:name/icon.png', async (request, response) => {
      const pathInternal = resolveInternal(
        LANGUAGES,
        request.params.name as string,
        LANG_ICON_PNG,
      );
      const pathExternal = resolveExternal(
        LANGUAGES,
        request.params.name as string,
        LANG_ICON_PNG,
      );
      if (await io.exists(pathInternal)) {
        response.sendFile(pathInternal);
      } else if (await io.exists(pathExternal)) {
        response.sendFile(pathExternal);
      } else {
        response.sendStatus(404);
      }
    });

  return router;
}
