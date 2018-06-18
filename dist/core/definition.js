"use strict";
/**
 * Definition
 */
Object.defineProperty(exports, "__esModule", { value: true });
const tslib_1 = require("tslib");
const logger = tslib_1.__importStar(require("./logger"));
const xml = tslib_1.__importStar(require("./xml"));
function parse(rowContents) {
    const bases = [];
    const definitions = [];
    Object.entries(rowContents).forEach(([path, raw]) => {
        let root;
        try {
            root = xml.parse(raw);
        }
        catch (error) {
            logger.error(error);
            return;
        }
        if (!root || !root.nodes) {
            logger.error(`Missing root element "Defs" or the Defs has no elements.\nfile: "${path}".`);
            return;
        }
    });
}
exports.parse = parse;
function isBase(def) {
    return !!def.attributes && !!def.attributes.Name;
}
function isAbstract(def) {
    return !!def.attributes && def.attributes.Abstract === 'True';
}
