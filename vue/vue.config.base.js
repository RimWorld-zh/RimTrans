const Config = require('webpack-chain');
const env = require('./env');
const cdnList = require('./pwa.cdn');

const baseOptions = pwaAssetsVersion => {
  return {
    runtimeCompiler: true,

    pwa: {
      assetsVersion: pwaAssetsVersion,
      workboxOptions: {
        importWorkboxFrom: 'local',
        cacheId: 'a9vg',
        runtimeCaching: [
          {
            urlPattern: '/',
            handler: 'networkFirst',
          },
          {
            urlPattern: '/static/*',
            handler: 'cacheFirst',
          },
          ...cdnList.map(urlPattern => ({
            urlPattern,
            handler: 'staleWhileRevalidate',
            options: {
              cacheableResponse: {
                statuses: [0, 206],
              },
            },
          })),
          {
            urlPattern: '/*',
            handler: 'networkFirst',
          },
        ],
      },
    },

    parallel: false,
  };
};

/**
 * @param {Config} config
 */
const baseChainWebpack = (config, options) => {
  const { isProd } = env();

  config.resolve.symlinks(true);
  config.resolve.alias.delete('@');

  // pwa --------------------------------------------------------

  if (isProd && config.plugins.has('workbox')) {
    config.plugin('workbox').tap(([opt]) => {
      opt.exclude.push(/\.html$/);

      return [opt];
    });
  }

  // html --------------------------------------------------------

  config.plugin('html').tap(([opt]) => [
    {
      ...opt,
      minify: false,
    },
  ]);

  // extra modules --------------------------------------------------------

  config.module
    .rule('graphql')
    .test(/.(gql|graphql)$/)
    .use('raw-loader')
    .loader('raw-loader');
};

module.exports = {
  baseOptions,
  baseChainWebpack,
};
