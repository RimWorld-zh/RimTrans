import fs from 'fs';
import { GenFilesOptions, genFiles, genPathResolve } from '@huiji/shared-utils';
import { genTypes } from './gen-types';
import { genProgress } from './gen-progress';

const resolvePath = genPathResolve(__dirname, '..');

const optionsList: GenFilesOptions[] = [
  {
    comments: ['All languages'],
    patterns: ['src/languages/**/*.ts'],
    output: 'src/languages/index.ts',
  },
];

Promise.all(optionsList.map(genFiles)).catch((e: Error) => {
  /* eslint-disable no-console */
  console.error(e.message);
  console.error(e.stack);
});

fs.writeFileSync(resolvePath('src', 'models-dict.ts'), genTypes());
fs.writeFileSync(resolvePath('src', 'progresses.ts'), genProgress());
