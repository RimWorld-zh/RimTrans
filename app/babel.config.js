module.exports = {
  presets: process.argv.includes('--pure')
    ? [
        [
          '@babel/preset-env',
          {
            targets: {
              node: '12.2.0',
            },
            modules: process.argv.includes('--esm') ? false : 'commonjs',
          },
        ],
        '@babel/preset-typescript',
      ]
    : [
        [
          '@vue/app',
          {
            targets: {
              electron: '5.0.6',
            },
          },
        ],
      ],
  sourceMaps: true,
  include: ['./src/**/*.ts', './src/**/*.tsx'],
  ignore:
    process.env.NODE_ENV === 'test'
      ? undefined
      : ['./src/**/*.test.ts', './src/**/*.test.tsx'],
};
