/**
 * Service main
 */
// tslint:disable:no-any no-unsafe-any
import pth from 'path';
import http from 'http';
import chalk from 'chalk';
import express from 'express';
import WebSocket from 'ws';
import { genPathResolve } from '@huiji/shared-utils';
import io from '@rimtrans/io';

import { PORT } from './constants';
import { WebSocketServer } from './sockets/utils-server';
import * as allListenerFactories from './sockets/all-server';

(async () => {
  const dataDir = 'rimtrans_data';
  const projectDir = __dirname.replace(
    /[\/\\]packages[\/\\]@rimtrans[\/\\]service.+/,
    '',
  );

  const internal = genPathResolve(projectDir, 'packages', '@rimtrans')('.');
  const external = (process as any).pkg
    ? genPathResolve(pth.dirname(process.execPath), dataDir)('.')
    : genPathResolve(projectDir, '.tmp', dataDir)('.');
  await io.createDirectory(external);

  console.log('internal', internal);
  console.log('external', external);

  const resolveStatic = genPathResolve(internal, 'ui', 'dist');

  const app = express();

  app
    .use('/static', express.static(resolveStatic('.')))
    .use('/static/*', (request, response) => response.sendStatus(404))
    .use('*', (request, response) => response.sendFile(resolveStatic('index.html')));

  const server = http.createServer(app);

  const wss = new WebSocket.Server({ server });

  wss.on('connection', (ws, request) => {
    const wrapper = new WebSocketServer(ws);
    wrapper.inject(internal, external, allListenerFactories);
  });

  server.listen(PORT, () =>
    console.info(`
${chalk.greenBright('Listening at')} ${chalk.cyanBright(`http://localhost:${PORT}`)}
Press ctrl+c to stop.
`),
  );
})();
