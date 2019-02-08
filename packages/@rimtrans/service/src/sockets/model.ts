// tslint:disable:no-any no-unsafe-any
import { LanguageCollection } from './all-model';

/**
 * Type Map for WebSocket key and data type, uses to server send to client
 */
export interface SocketDataMapToClient {
  configs: any;
  languageCollection: LanguageCollection;
}

/**
 * Type Map for WebSocket key and data type, uses to client send to server
 */
export interface SocketDataMapToServer {
  configs: any;
  languageCollection: 'update';
}
