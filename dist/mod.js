"use strict";
/**
 * Models for RimWorld.
 */
Object.defineProperty(exports, "__esModule", { value: true });
var ContentSource;
(function (ContentSource) {
    // basic
    ContentSource[ContentSource["Undefined"] = 0] = "Undefined";
    ContentSource[ContentSource["LocalFolder"] = 1] = "LocalFolder";
    ContentSource[ContentSource["SteamWorkshop"] = 2] = "SteamWorkshop";
    // extended
    ContentSource[ContentSource["Remote"] = 3] = "Remote";
})(ContentSource = exports.ContentSource || (exports.ContentSource = {}));
