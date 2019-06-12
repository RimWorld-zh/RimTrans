module.exports = {
  presets: [
    [
      '@babel/preset-env',
      {
        targets: {
          node: '12.2.0',
        },
      },
    ],
    '@babel/preset-typescript',
  ],
};
