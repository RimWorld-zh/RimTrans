import * as io from '@rimtrans/io';
import { PrettierOptions, resolveXmlPrettierOptions, regexEndOfLine } from './xml';

export class StringsFile {
  /**
   * Load `Strings` text files of mods.
   * @param stringsDirectories tha array of paths to `Strings` directories of mods.
   */
  public static async load(
    stringsDirectories: string[],
  ): Promise<Record<string, string>[]> {
    return Promise.all(
      stringsDirectories.map(async dir => {
        const map: Record<string, string> = {};

        if (!(await io.directoryExists(dir))) {
          return map;
        }

        await io
          .search(['**/*.txt'], {
            cwd: dir,
            case: false,
            onlyFiles: true,
          })
          .then(files =>
            Promise.all(
              files.map(async fileName => {
                map[fileName] = await io.read(io.join(dir, fileName));
              }),
            ),
          );

        return map;
      }),
    );
  }

  /**
   * Merge the old translation strings map to the origin strings map, and return a new map.
   * @param originMap the origin(English) strings map
   * @param oldMap the old translation strings map
   */
  public static merge(
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
  public static async save(
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
