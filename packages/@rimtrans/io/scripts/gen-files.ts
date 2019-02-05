/**
 * Auto generate files
 */
import { genFiles } from '@huiji/shared-utils';

genFiles({
  comments: ['All modules'],
  patterns: ['src/*.ts', '!src/index.ts'],
  output: 'src/all.ts',
});
