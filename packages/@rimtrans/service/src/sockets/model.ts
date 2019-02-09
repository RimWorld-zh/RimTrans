// tslint:disable:no-any no-unsafe-any
import { Configs, LanguageCollection } from './all-model';

/**
 * Type Map for WebSocket key and data type, uses to server send to client
 */
export interface SocketDataMapToClient {
  configs: Configs;
  languageCollection: LanguageCollection;
}

/**
 * Type Map for WebSocket key and data type, uses to client send to server
 */
export interface SocketDataMapToServer {
  configs: Configs;
  languageCollection: 'update';
}
