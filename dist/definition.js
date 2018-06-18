"use strict";
/**
 * Definition
 */
Object.defineProperty(exports, "__esModule", { value: true });
const tslib_1 = require("tslib");
const logger = tslib_1.__importStar(require("./logger"));
const xml = tslib_1.__importStar(require("./xml"));
function parse(rowContents) {
    const data = {
        abstracts: {},
        definitions: {},
    };
    Object.entries(rowContents).forEach(([path, raw]) => {
        const doc = xml.parse(raw);
        const defs = doc.nodes &&
            doc.nodes.find(e => e.type === xml.NodeType.element && e.name === 'Defs');
        if (!defs || !defs.nodes) {
            logger.error(`Missing root element "Defs" or the Defs has no elements.\nfile: "${path}".`);
            return;
        }
        defs.nodes.reduce((pre, cur) => {
            return cur;
        });
    });
}
exports.parse = parse;
