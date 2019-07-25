module.exports = {
  root: true,
  env: {
    node: true,
  },
  extends: ['../.eslintrc.js'],
  rules: {
    'no-console': process.env.NODE_ENV === 'production' ? 'error' : 'off',
    'no-debugger': process.env.NODE_ENV === 'production' ? 'error' : 'off',
  },
};
