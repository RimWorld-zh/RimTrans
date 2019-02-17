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

import { PORT, BASE_URL_STATIC, BASE_URL_REST_API } from './api/all-model';
import { WebSocketServer } from './api/utils-server';
import * as allListenerFactories from './api/all-server';
import * as allRestRouters from './api/all-handler';

async function setup(): Promise<void> {
  const dataDir = 'rimtrans_data';
  const projectDir = __dirname.replace(
    /[\/\\]packages[\/\\]@rimtrans[\/\\]service.+/,
    '',
  );
  const internal = genPathResolve(projectDir, 'packages', '@rimtrans')('.');
  const external = (process as any).pkg
    ? genPathResolve(pth.dirname(process.execPath), dataDir)('.')
    : genPathResolve(projectDir, '.tmp', dataDir)('.');
  const resolveStatic = genPathResolve(internal, 'ui', 'dist');

  await io.createDirectory(external);

  // tslint:disable:no-console
  console.log('internal', internal);
  console.log('external', external);
  // tslint:enable:no-console

  const app = express()
    .use(
      BASE_URL_REST_API,
      Object.entries(allRestRouters).reduce<express.Router>(
        (router, [name, subRouter]) => router.use(`/${name}`, subRouter),
        express.Router(),
      ),
    )
    .use(`${BASE_URL_REST_API}/*`, (request, response) => response.sendStatus(404))
    .use(BASE_URL_STATIC, express.static(resolveStatic('.')))
    .use(`${BASE_URL_STATIC}/*`, (request, response) => response.sendStatus(404))
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
}

setup().catch(console.error);
