import { GenFilesOptions, genFiles } from '@huiji/shared-utils';

const optionsList: GenFilesOptions[] = [
  // renderer
  // components
  {
    comments: ['All components'],
    patterns: [
      'src/renderer/components/**/*.ts',
      'src/renderer/components/**/*.tsx',
      '!src/renderer/components/index.ts',
      '!src/renderer/components/base/**/*.ts',
    ],
    output: 'src/renderer/components/all.ts',
  },
  {
    comments: ['All components style'],
    patterns: [
      'src/renderer/components/**/*.scss',
      '!src/renderer/components/base/**/*.scss',
    ],
    output: 'src/renderer/components/index.scss',
  },
  // views
  {
    comments: ['All views style'],
    patterns: ['src/renderer/views/**/*.scss'],
    output: 'src/renderer/views/index.scss',
  },
  // utils
  {
    comments: ['All utils'],
    patterns: ['src/renderer/utils/**/*.ts'],
    output: 'src/renderer/utils/index.ts',
  },

  // main
  // utils
  {
    comments: ['All utils'],
    patterns: ['src/main/utils/**/*.ts'],
    output: 'src/main/utils/index.ts',
  },
];

Promise.all(optionsList.map(genFiles)).catch((e: Error) => {
  /* eslint-disable no-console */
  console.error(e.message);
  console.error(e.stack);
});
