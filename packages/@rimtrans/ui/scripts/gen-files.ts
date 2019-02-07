/**
 * Auto generate index files
 */

import { GenFilesOptions, genFiles } from '@huiji/shared-utils';

const optionsList: GenFilesOptions[] = [
  {
    comments: ['All components'],
    patterns: ['ts', 'tsx'].map(ext => `src/components/**/*.${ext}`),
    output: 'src/components/all.ts',
  },
  {
    comments: ['All components style'],
    patterns: ['src/components/**/*.scss'],
    output: 'src/components/all.scss',
  },
  {
    comments: ['All views style'],
    patterns: ['src/views/**/*.scss'],
    output: 'src/views/all.scss',
  },
];

Promise.all(optionsList.map(async opts => genFiles(opts))).catch((error: Error) => {
  console.error(error.message);
  console.error(error.stack);
});
