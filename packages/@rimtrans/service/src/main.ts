/**
 * Service main
 */

import pth from 'path';
import express from 'express';
import { genPathResolve } from '@huiji/shared-utils';

import io from '@rimtrans/io';

import * as allHandlers from './middlewares/all-handlers';

const PORT = 5100;

(async () => {
  const resolveInternal = genPathResolve(__dirname, '../../..');
  const resolveExternal = __dirname.startsWith('/snapshot')
    ? genPathResolve(pth.dirname(process.execPath), 'rimtrans_data')
    : genPathResolve(__dirname, '../../../../../.tmp/data');

  const CORE_INTERNAL = resolveInternal('core');
  const CORE_EXTERNAL = resolveExternal('core');
  await io.createDirectory(CORE_EXTERNAL);

  const app = express();

  app
    .use(
      '/api',
      express
        .Router()
        .use('/core', allHandlers.handlerCore(CORE_INTERNAL, CORE_EXTERNAL)),
    )
    .use('*', (request, response) =>
      response.send(`Hello world! Service is running in ${__dirname}`),
    )
    .listen(PORT, () => {
      console.info(`Service is listening at localhost:${PORT}`);
    });
})();
