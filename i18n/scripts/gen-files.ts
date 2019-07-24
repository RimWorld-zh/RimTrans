import { GenFilesOptions, genFiles } from '@huiji/shared-utils';

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
