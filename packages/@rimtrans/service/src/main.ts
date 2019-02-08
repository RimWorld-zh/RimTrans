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
  const external = __dirname.startsWith('/snapshot')
    ? genPathResolve(pth.dirname(process.execPath), 'rimtrans_data')('.')
    : genPathResolve(__dirname, '../../../../../.tmp/data')('.');

  const app = express();

  app.use('*', (request, response) =>
    response.send(`Hello world! Service is running in ${__dirname}`),
  );

  const server = http.createServer(app);

  const wss = new WebSocket.Server({ server });

  wss.on('connection', (ws, request) => {
    const wrapper = new WebSocketServer(ws);
    wrapper.inject(internal, external, allListenerFactories);
  });

  server.listen(PORT, () => console.info(`Listening at *:${PORT}`));
})();
