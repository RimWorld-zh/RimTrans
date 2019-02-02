/**
 * @param {string} envName
 * @param {string} value
 * @param {string[]} available
 */
function validateEnv(envName, value, available) {
  if (!available.includes(value)) {
    throw new Error(
      `Invalid env ${envName}='${value}', it must be one of ${available.join(', ')}`,
    );
  }
}

module.exports = function() {
  const isProd = process.env.NODE_ENV === 'production';
  const isLegacyBundle =
    process.env.VUE_CLI_MODERN_MODE && !process.env.VUE_CLI_MODERN_BUILD;

  const port = parseInt(process.env.VUE_APP_PORT, 10);
  if (!port) {
    throw new Error(`Invalid port: ${process.env.VUE_APP_SIDE}`);
  }

  return {
    isProd,
    isLegacyBundle,
    port,
  };
};
