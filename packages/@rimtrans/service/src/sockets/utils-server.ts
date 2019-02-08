/**
 * Utils for server side request handlers
 */
// tslint:disable:no-any no-unsafe-any
import pth from 'path';
import globby from 'globby';
import WebSocket from 'ws';
import { genPathResolve } from '@huiji/shared-utils';
import io from '@rimtrans/io';
import { SocketDataMapToClient, SocketDataMapToServer } from './model';

const log = (...args: any[]) => {
  console.info(new Date().toISOString(), 'WS', ...args);
};

export type ServerListenerFactory<K extends keyof SocketDataMapToServer = any> = (
  internal: string,
  external: string,
) => ServerListener<K>;

/**
 * Listener function type for server
 */
export type ServerListener<K extends keyof SocketDataMapToServer> = (
  wss: WebSocketServer,
  data?: SocketDataMapToServer[K],
) => any;

/**
 * The WebSocket wrapper for server side
 */
export class WebSocketServer {
  private ws: WebSocket;
  private listenerMap: Record<string, ((wss: this, data?: any) => any)[] | undefined>;

  /**
   * @param ws the WebSocket instance (npm module `ws`)
   */
  constructor(ws: WebSocket) {
    this.listenerMap = {};
    this.ws = ws;
    this.ws.on('message', raw => this.listen(raw));
    this.ws.on('close', () => this.destructor());
  }
  private listen(raw: any): void {
    const { key, data } = JSON.parse(raw);

    log('listen', key, data);

    if (typeof key === 'string' && key) {
      const list = this.listenerMap[key];
      if (list) {
        list.forEach(l => l(this, data));
      } else {
        log('listen', `key not found: ${key}`);
      }
    }
  }

  private destructor(): void {
    Object.keys(this.listenerMap).forEach(key => {
      this.listenerMap[key] = undefined;
    });
  }

  /**
   * Wrap `data` with `key` and send to client
   * @param key the key
   * @param data
   */
  public send<K extends keyof SocketDataMapToClient>(
    key: K,
    data?: SocketDataMapToClient[K],
  ): void {
    log('send', key, data);

    this.ws.send(JSON.stringify({ key, data }));
  }

  public inject(
    internal: string,
    external: string,
    factories: Record<string, ServerListenerFactory>,
  ): void {
    Object.entries(factories).forEach(([key, factory]) => {
      this.addListener(key, factory(internal, external));
    });
  }

  /**
   * Add a listener to receive data from client massage
   * @param key the key
   * @param listener the listener
   */
  public addListener<K extends keyof SocketDataMapToServer>(
    key: K,
    listener: ServerListener<K>,
  ): void {
    const list = (this.listenerMap[key] ||
      (this.listenerMap[key] = [])) as ServerListener<K>[];
    list.push(listener);
  }

  /**
   * Remove the listener
   * @param key the key
   * @param listener the listener
   */
  public removeListener<K extends keyof SocketDataMapToServer>(
    key: K,
    listener: ServerListener<K>,
  ): void {
    const list = this.listenerMap[key];
    if (list) {
      this.listenerMap[key] = list.filter(l => l !== listener);
    }
  }
}

export async function copyModFiles(
  src: string,
  dest: string,
  patterns: string[],
): Promise<void> {
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
