/**
 * Service main
 */

import express from 'express';
import { genPathResolve } from '@huiji/shared-utils';

import io from '@rimtrans/io';

import * as allHandlers from './middlewares/all-handlers';

const PORT = 5100;

(async () => {
  const resolveDir = genPathResolve(__dirname, '..', '..');
  const resolveCwd = genPathResolve(process.cwd());

  const app = express();

  const api = express.Router();

  const coreInternal = resolveDir('..', 'core');
  const coreExternal = resolveCwd('data', 'core');
  await io.createDirectory(coreExternal);
  api.use('/core', allHandlers.handlerCore(coreInternal, coreExternal));

  app.use('/api', api);

  app.use('*', (request, response) =>
    response.send(`Hello world! Service is running in ${__dirname}`),
  );

  app.listen(PORT, () => {
    console.info(`Service is listening at localhost:${PORT}`);
  });
})();
