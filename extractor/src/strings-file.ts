import * as io from '@rimtrans/io';
import { ExtractorEventEmitter, Progress } from './extractor-event-emitter';
import { PrettierOptions, resolveXmlPrettierOptions, regexEndOfLine } from './xml';

export class StringsFileExtractor {
  /* eslint-disable lines-between-class-members */
  public readonly ACTION_LOAD = 'Strings Load';
  public readonly ACTION_SAVE = 'Strings Save';
  /* eslint-enable lines-between-class-members */

  private readonly emitter: ExtractorEventEmitter;

  public constructor(emitter: ExtractorEventEmitter) {
    this.emitter = emitter;
  }

  /**
   * Load `Strings` text files of mods.
   * @param stringsDirectories tha array of paths to `Strings` directories of mods.
   */
  public async load(stringsDirectories: string[]): Promise<Record<string, string>[]> {
    const action = this.ACTION_LOAD;

    return Promise.all(
      stringsDirectories.map(async dir => {
        this.emitter.emit('progress', {
          action,
          key: dir,
          status: 'pending',
          info: 'loading',
        });

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

        this.emitter.emit('progress', {
          action,
          key: dir,
          status: 'succeed',
          info: 'loaded',
        });

        return map;
      }),
    );
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
    const action = this.ACTION_SAVE;
    this.emitter.emit('progress', {
      action,
      key: directory,
      status: 'pending',
      info: 'saving',
    });

    const { eol } = resolveXmlPrettierOptions(prettierOptions);

    await Promise.all(
      Object.entries(stringsMap).map(([fileName, content]) =>
        io.save(io.join(directory, fileName), content.replace(regexEndOfLine, eol)),
      ),
    );

    this.emitter.emit('progress', {
      action,
      key: directory,
      status: 'succeed',
      info: 'saved',
    });
  }
}
