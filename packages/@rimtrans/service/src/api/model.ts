// tslint:disable:no-any no-unsafe-any
import { Configs, LanguageCollection } from './all-model';

// Constants

/**
 * The port to listen
 */
export const PORT = 5100;
export const BASE_URL_REST_API = '/rest';
export const BASE_URL_WEB_SOCKET = '/ws';

// WebSocket

/**
 * Type Map for WebSocket key and data type, uses to server send to client
 */
export interface SocketDataMapToClient {
  configs: Configs;
  coreLanguages: LanguageCollection;
}

/**
 * Type Map for WebSocket key and data type, uses to client send to server
 */
export interface SocketDataMapToServer {
  configs: Configs;
  coreLanguages: 'update';
}
