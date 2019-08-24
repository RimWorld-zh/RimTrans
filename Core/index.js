/* eslint-disable */
const core = require('./type-package.json');
const coreFix = require('./type-package-fix.json');

function getCoreTypePackage() {
  return JSON.parse(
    JSON.stringify({
      ...core,
      ...coreFix,
    }),
  );
}

function getCorePath() {
  return __dirname;
}

module.exports.getCoreTypePackage = getCoreTypePackage;
module.exports.getCorePath = getCorePath;
module.exports.RimWorldVersion = '1.0.2282';
