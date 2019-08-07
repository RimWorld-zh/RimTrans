/* eslint-disable @typescript-eslint/no-var-requires */
const Config = require('webpack-chain');
const { genPathResolve } = require('@huiji/shared-utils');

const resolvePath = genPathResolve(__dirname);

const isDev = process.env.NODE_ENV === 'development';
const isProd = process.env.NODE_ENV === 'production';
const publicPath = './';

module.exports = {
  parallel: false,
  lintOnSave: !isProd,

  outputDir: 'dist/renderer',
  filenameHashing: false,
  publicPath,
  runtimeCompiler: true,

  /**
   * @param {Config} config
   */
  chainWebpack: config => {
    config.target('electron-renderer');

    // config.resolve.symlinks(false);

    config.resolve.alias.delete('@').set('@src', resolvePath('src'));

    if (isDev) {
      config.resolve.alias.set('@rimtrans/i18n$', resolvePath('..', 'i18n', 'src'));
    }

    config
      .entry('app')
      .clear()
      .add('./src/renderer/renderer.ts');
  },

  devServer: {
    port: process.env.VUE_APP_PORT,
    open: false,
  },
};
