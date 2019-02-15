import express from 'express';
import globby from 'globby';

import { ModsParams } from './model';
import io from '@rimtrans/io';
import { genPathResolve } from '@huiji/shared-utils';

/**
 * Route handler for mod
 */
const mods = express.Router();

mods.get('/', async (request, response) => {
  const { path } = request.query as ModsParams;
  if (!path) {
    response.sendStatus(400);
  }

  const resolvePath = genPathResolve(path);
  const files = await globby('*/About/About.xml', {
    cwd: path,
    onlyFiles: true,
  });

  const entries = await Promise.all(
    files.map(async f => {
      const content = await io.load(resolvePath(f), true);

      return [f.replace(/[\/\\]About[\/\\]About.xml/, ''), content];
    }),
  );

  response.send(entries);
});

export { mods };
