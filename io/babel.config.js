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
  sourceMaps: true,
  include: ['./src/**/*.ts'],
  ignore: process.env.NODE_ENV === 'test' ? undefined : ['./src/**/*.test.ts'],
};
