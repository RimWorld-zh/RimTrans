module.exports = {
  moduleFileExtensions: ['ts', 'tsx', 'js', 'jsx', 'json'],
  transform: {
    '^.+\\.tsx?$': 'ts-jest',
  },
  moduleNameMapper: {
    '^@/(.*)$': '<rootDir>/src/$1',
  },
  testMatch: [
    '**/__tests__/**/*.js?(x)',
    '**/?(*.)(spec|test).js?(x)',
    '**/__tests__/**/*.ts?(x)',
    '**/?(*.)(spec|test).ts?(x)',
  ],
};
