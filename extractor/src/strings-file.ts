import * as io from '@rimtrans/io';
import { ExtractorEventEmitter } from './extractor-event-emitter';
import { PrettierOptions, resolveXmlPrettierOptions, regexEndOfLine } from './xml';

export class StringsFileExtractor {
  private readonly emitter: ExtractorEventEmitter;

  public constructor(emitter: ExtractorEventEmitter) {
    this.emitter = emitter;
  }

  /**
   * Load `Strings` text files of the mod.
   * @param directory path to the directory 'Strings' of the mod for the specified language
   */
  public async load(directory: string): Promise<Record<string, string>> {
    const files = await io.search(['**/*.txt'], {
      cwd: directory,
      caseSensitiveMatch: false,
      onlyFiles: true,
    });

    const map: Record<string, string> = {};

    await Promise.all(
      files.map(async fileName => {
        map[fileName] = await io.read(io.join(directory, fileName));
      }),
    );

    return map;
  }

  /**
   * Merge the old translation strings map to the origin strings map, and return a new map.
   * @param originMap the origin(English) strings map
   * @param oldMap the old translation strings map
   */
  public merge(
    originMap: Record<string, string>,
    oldMap: Record<string, string>,
  ): Record<string, string> {
    const newMap: Record<string, string> = Object.fromEntries(Object.entries(originMap));

    Object.entries(oldMap).forEach(([fileName, content]) => {
      newMap[fileName] = content;
    });

    return newMap;
  }

  /**
   * Save strings map as text files.
   * @param directory the directory to save to
   * @param stringsMap the strings map
   * @param prettierOptions format options
   */
  public async save(
    directory: string,
    stringsMap: Record<string, string>,
    prettierOptions?: PrettierOptions,
  ): Promise<void> {
    const { eol } = resolveXmlPrettierOptions(prettierOptions);

    await Promise.all(
      Object.entries(stringsMap).map(([fileName, content]) =>
        io.save(io.join(directory, fileName), content.replace(regexEndOfLine, eol)),
      ),
    );
  }
}
