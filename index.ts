/**
 * Test XML
 */

const log: typeof console.log = console.log;

import fs from 'fs';
import globby from 'globby';

import * as xml from './core/xml';
import * as definition from './core/definition';

const DEFS_PATH: string = '/mnt/f/rw/RimWorld-Core/Core/Defs';

const rawContents: xml.RawContentMap = {};

globby
  .sync(`${DEFS_PATH}/**/*.xml`)
  .forEach(p => (rawContents[p] = fs.readFileSync(p, 'utf-8')));

definition.parse(rawContents);
