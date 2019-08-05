import { GenFilesOptions, genFiles } from '@huiji/shared-utils';
import { genVariables } from './gen-variables';

const optionsList: GenFilesOptions[] = [
  // main process
  // utils
  {
    comments: ['All utils'],
    patterns: ['src/main/utils/**/*.ts'],
    output: 'src/main/utils/index.ts',
  },

  // renderer process
  // components
  {
    comments: ['All components'],
    patterns: [
      'src/renderer/components/**/*.ts',
      'src/renderer/components/**/*.tsx',
      '!src/renderer/components/**/_*.ts',
      '!src/renderer/components/**/_*.tsx',
      '!src/renderer/components/base/**/*.ts',
    ],
    output: 'src/renderer/components/index.ts',
  },
  {
    comments: ['All components style'],
    patterns: [
      'src/renderer/components/**/*.scss',
      '!src/renderer/components/**/_*.scss',
      '!src/renderer/components/base/**/*.scss',
    ],
    output: 'src/renderer/components/index.scss',
  },
  // services
  {
    comments: ['All services'],
    patterns: ['src/renderer/services/**/*.ts', 'src/renderer/services/**/*.tsx'],
    output: 'src/renderer/services/index.ts',
  },
  {
    comments: ['All services style'],
    patterns: ['src/renderer/services/**/*.scss'],
    output: 'src/renderer/services/index.scss',
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
];

Promise.all([genVariables(), ...optionsList.map(genFiles)]).catch((e: Error) => {
  /* eslint-disable no-console */
  console.error(e.message);
  console.error(e.stack);
});
