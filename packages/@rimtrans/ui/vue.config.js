const options = {
  publicPath: process.env.NODE_ENV === 'production' ? '/static/' : '/',

  devServer: {
    port: 5102,
    open: true,
    proxy: {
      '/api': {
        target: 'http://localhost:5100',
        changeOrigin: true,
      },
    },
  },
};

module.exports = options;
