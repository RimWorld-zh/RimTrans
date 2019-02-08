/**
 * Service main
 */

import pth from 'path';
import http from 'http';
import express from 'express';
import WebSocket from 'ws';
import { genPathResolve } from '@huiji/shared-utils';
import io from '@rimtrans/io';

import { PORT } from './constants';
import { WebSocketServer } from './sockets/utils-server';
import * as allListenerFactories from './sockets/all-server';

(async () => {
  const internal = genPathResolve(__dirname, '../../..')('.');
  const external = (process as any).pkg
    ? genPathResolve(pth.dirname(process.execPath), 'rimtrans_data')('.')
    : genPathResolve(__dirname, '../../../../../.tmp/data')('.');
  await io.createDirectory(external);
  console.log(internal);
  console.log(external);

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

  server.listen(PORT, () => console.info(`Listening at *:${PORT}`));
})();
