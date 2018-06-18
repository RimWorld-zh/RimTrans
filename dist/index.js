"use strict";
/**
 * Test XML
 */
Object.defineProperty(exports, "__esModule", { value: true });
const tslib_1 = require("tslib");
const log = console.log;
const fs_1 = tslib_1.__importDefault(require("fs"));
const globby_1 = tslib_1.__importDefault(require("globby"));
const xml = tslib_1.__importStar(require("./core/xml"));
const definition = tslib_1.__importStar(require("./core/definition"));
const DEFS_PATH = '/mnt/f/rw/RimWorld-Core/Core/Defs';
log(JSON.stringify(xml.parse(' <b>sdf</b><b>sdf</b>  \n')));
log('start');
const rawContents = {};
globby_1.default
    .sync(`${DEFS_PATH}/**/*.xml`)
    .forEach(p => (rawContents[p] = fs_1.default.readFileSync(p, 'utf-8')));
definition.parse(rawContents);
