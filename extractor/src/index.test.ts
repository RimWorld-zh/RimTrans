import {
  fse,
  pth,
  globby,
  Extractor,
  DefinitionExtractor,
  InjectionExtractor,
  KeyedReplacementExtractor,
  StringsFileExtractor,
} from './index';

describe('index', () => {
  test('third-part modules wrapper', () => {
    expect(pth).toBeDefined();
    expect(pth.join).toBeDefined();
    expect(typeof pth.join).toBe('function');

    expect(fse).toBeDefined();
    expect(fse.outputFile).toBeDefined();
    expect(typeof fse.outputFile).toBe('function');

    expect(globby).toBeDefined();
    expect(typeof globby).toBe('function');
    expect(globby.sync).toBeDefined();
    expect(typeof globby.sync).toBe('function');
  });

  test('exports', () => {
    expect(Extractor).toBeDefined();
    expect(DefinitionExtractor).toBeDefined();
    expect(InjectionExtractor).toBeDefined();
    expect(KeyedReplacementExtractor).toBeDefined();
    expect(StringsFileExtractor).toBeDefined();
  });
});
