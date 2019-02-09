/**
 * Utils for client WebSocket
 */
// tslint:disable:no-any no-unsafe-any
import { SocketDataMapToClient, SocketDataMapToServer } from './model';

/**
 * Listener function type for client
 */
export type ClientListener<K extends keyof SocketDataMapToClient> = (
  data?: SocketDataMapToClient[K],
) => any;

/**
 * The WebSocket wrapper for client side
 */
export class WebSocketClient {
  public readonly ws: WebSocket;
  private listenerMap: Record<string, ((data?: any) => any)[] | undefined>;

  /**
   * @param ws the WebSocket instance (browser native)
   */
  constructor(ws: WebSocket) {
    this.listenerMap = {};
    this.ws = ws;
    this.ws.addEventListener('message', event => this.listen(event));
  }

  private listen(event: MessageEvent): void {
    const { key, data } = JSON.parse(event.data);
    console.log('WS', key, data);
    if (typeof key === 'string' && key) {
      const list = this.listenerMap[key];
      if (list) {
        list.forEach(l => l(data));
      }
    }
  }

  /**
   * Wrap `data` with `key` and send to server
   * @param key the key
   * @param data the data
   */
  public send<K extends keyof SocketDataMapToServer>(
    key: K,
    data?: SocketDataMapToServer[K],
  ): void {
    this.ws.send(JSON.stringify({ key, data }));
  }

  /**
   * Add a listener to receive data from server massage
   * @param key the key
   * @param listener the listener function
   */
  public addListener<K extends keyof SocketDataMapToClient>(
    key: K,
    listener: ClientListener<K>,
  ): void {
    const list = (this.listenerMap[key] ||
      (this.listenerMap[key] = [])) as ClientListener<K>[];
    list.push(listener);
  }

  /**
   * Remove the listener
   * @param key the key
   * @param listener the listener function
   */
  public removeListener<K extends keyof SocketDataMapToClient>(
    key: K,
    listener: ClientListener<K>,
  ): void {
    const list = this.listenerMap[key];
    if (list) {
      this.listenerMap[key] = list.filter(l => l !== listener);
    }
  }
}

export const wsc = new WebSocketClient(new WebSocket(`ws://${location.host}/ws`));
