import {
  Extractor,
  DefinitionExtractor,
  InjectionExtractor,
  KeyedReplacementExtractor,
  StringsFileExtractor,
} from './index';

describe('index', () => {
  test('exports', () => {
    expect(Extractor).toBeDefined();
    expect(DefinitionExtractor).toBeDefined();
    expect(InjectionExtractor).toBeDefined();
    expect(KeyedReplacementExtractor).toBeDefined();
    expect(StringsFileExtractor).toBeDefined();
  });
});
