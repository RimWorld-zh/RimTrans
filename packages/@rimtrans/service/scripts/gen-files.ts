/**
 * Auto generate index files
 */

import { GenFilesOptions, genFiles } from '@huiji/shared-utils';

const optionsList: GenFilesOptions[] = [
  {
    comments: ['All Models'],
    patterns: ['src/models/**/*.ts'],
    output: 'src/models/index.ts',
  },
  ...['model', 'client', 'server'].map<GenFilesOptions>(item => ({
    comments: [`All WebSocket ${item}s`],
    patterns: [`src/sockets/**/${item}.ts`],
    output: `src/sockets/all-${item}.ts`,
  })),
];

Promise.all(optionsList.map(async opts => genFiles(opts))).catch((error: Error) => {
  console.error(error.message);
  console.error(error.stack);
});
