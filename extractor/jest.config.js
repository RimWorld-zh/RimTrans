// For a detailed explanation regarding each configuration property, visit:
// https://jestjs.io/docs/en/configuration.html

module.exports = {
  collectCoverage: true,
  coveragePathIgnorePatterns: ['/node_modules/', '/lib/', 'utils.ts', 'utils.test.ts'],
  coverageDirectory: 'coverage',
  coverageThreshold: {
    global: {
      statements: 100,
      branches: 100,
      functions: 100,
      lines: 100,
    },
  },
  moduleDirectories: ['node_modules'],
  moduleFileExtensions: ['js', 'json', 'jsx', 'ts', 'tsx'],
  testEnvironment: 'node',
  testPathIgnorePatterns: ['/node_modules/', '/lib/', 'print.ts', 'utils.test.ts'],
  verbose: true,
};
