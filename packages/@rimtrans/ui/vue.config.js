const options = {
  publicPath: process.env.NODE_ENV === 'production' ? '/static/' : '/',

  devServer: {
    port: process.env.VUE_APP_PORT,
    open: true,
  },
};

module.exports = options;
