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
/**
 * Parse the xml document text and return the root element.
 * @param rawContent the plain text of the xml document.
 */
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
    if (doc.nodes) {
        return doc.nodes.find(n => n.type === NodeType.element);
    }
}
exports.parse = parse;
function clone(node) {
    return JSON.parse(JSON.stringify(node));
}
exports.clone = clone;
function findElement(parent, name) {
    if (parent && parent.type === NodeType.element && parent.nodes) {
        return parent.nodes.find(el => el.type === NodeType.element && el.name === name);
    }
}
exports.findElement = findElement;
