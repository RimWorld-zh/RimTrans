"use strict";
/**
 * Utils for xmljs
 */
// tslint:disable:no-reserved-keywords
Object.defineProperty(exports, "__esModule", { value: true });
const tslib_1 = require("tslib");
const xml_js_1 = tslib_1.__importDefault(require("xml-js"));
var NodeType;
(function (NodeType) {
    NodeType["comment"] = "comment";
    NodeType["element"] = "element";
    NodeType["text"] = "text";
})(NodeType = exports.NodeType || (exports.NodeType = {}));
function parse(rawContent) {
    const doc = xml_js_1.default.xml2js(rawContent, {
        compact: false,
        trim: false,
        nativeType: false,
        addParent: false,
        alwaysArray: true,
        alwaysChildren: true,
        ignoreDeclaration: true,
        ignoreText: false,
        elementsKey: 'nodes',
    });
    return doc.nodes
        ? doc.nodes.find(n => n.type === NodeType.element)
        : undefined;
}
exports.parse = parse;
function clone(xml) {
    return JSON.parse(JSON.stringify(xml));
}
exports.clone = clone;
function findElement() { }
exports.findElement = findElement;
