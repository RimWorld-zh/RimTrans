import { ModMetaData } from '@rimtrans/extractor';

declare module '../../utils/ipc' {
  interface IpcTypeMap {
    'mod-meta-data': ['local' | 'steam' | string[], Record<string, ModMetaData>];
  }
}

export interface ModMetaDataSlaver {
  request: [
    {
      genre: 'directories' | 'files';
      paths: string[];
    },
    Record<string, ModMetaData>,
  ];
}
