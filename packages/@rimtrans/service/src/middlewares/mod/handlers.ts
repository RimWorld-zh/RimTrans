/**
 * Handler Core
 */
import fs from 'fs';
import express from 'express';
import globby from 'globby';
import { genPathResolve } from '@huiji/shared-utils';
import { languageInfos, LanguageManifest } from '@rimtrans/core';
import io, { load } from '@rimtrans/io';
import { setContentType } from '../utils-handlers';
import { ABOUT, ABOUT_XML, PREVIEW_PNG, DEFS, LANGUAGES } from './models';

/**
 * Generate a router handler for Core
 * @param internal the path to the internal directory for Core
 * @param external the path to the external directory for Core
 */
export function handlerCore(internal: string, external: string): express.Router {
  const router = express.Router();

  const resolveInternal = genPathResolve(internal);
  const resolveExternal = genPathResolve(external);

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
      const languageTimestamps: Record<string, number> = {};
      languageInfos.forEach(info => (languageTimestamps[info.name] = 0));

      for (const resolvePath of [resolveInternal, resolveExternal]) {
        const languages = await globby('*', {
          cwd: resolvePath(LANGUAGES),
          onlyDirectories: true,
        });
        await Promise.all(
          languages.map(async l => {
            if (typeof languageTimestamps[l] !== 'number') {
              languageTimestamps[l] = 0;
            }
            try {
              const manifestPath = resolvePath(LANGUAGES, l, 'manifest.json');
              if (fs.existsSync(manifestPath)) {
                const manifest = JSON.parse(
                  await load(manifestPath, true),
                ) as LanguageManifest;
                if (
                  typeof manifest.timestamp === 'number' &&
                  manifest.timestamp > 0 &&
                  languageTimestamps[l] < manifest.timestamp
                ) {
                  languageTimestamps[l] = manifest.timestamp;
                }
              }
            } catch (error) {
              // TODO
            }
          }),
        );
      }

      response.send(languageTimestamps);
    });

  return router;
}
