import { Configs } from './all-model';

/**
 * Type Map for WebSocket key and data type, uses to server send to client
 */
export interface SocketDataMapToClient {
  configs: Configs;
}

/**
 * Type Map for WebSocket key and data type, uses to client send to server
 */
export interface SocketDataMapToServer {
  configsInit: never;
  configs: Configs;
}
