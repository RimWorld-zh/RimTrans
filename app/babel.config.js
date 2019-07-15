module.exports = {
  presets: [
    [
      '@babel/preset-env',
      {
        targets: {
          electron: '5.0.6',
        },
      },
    ],
    '@babel/preset-typescript',
  ],
  sourceMaps: true,
  include: ['./src/**/*.ts'],
  ignore: process.env.NODE_ENV === 'test' ? undefined : ['./src/**/*.test.ts'],
};
