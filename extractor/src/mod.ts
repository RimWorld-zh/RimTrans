import * as io from '@rimtrans/io';
import { loadXML } from './xml';
import {
  ID_CORE,
  DEFAULT_LANGUAGE,
  FILE_NAME_ABOUT,
  FILE_NAME_PREVIEW,
  FILE_NAME_PUBLISHED_FILE_ID,
  FOLDER_NAME_ABOUT,
  FOLDER_NAME_ASSEMBLIES,
  FOLDER_NAME_DEFS,
  FOLDER_NAME_PATCHES,
  FOLDER_NAME_LANGUAGES,
  FOLDER_NAME_TEXTURES,
  FOLDER_NAME_BACKSTORIES,
  FOLDER_NAME_DEF_INJECTED,
  FOLDER_NAME_KEYED,
  FOLDER_NAME_STRINGS,
} from './constants';

// Constants

/**
 * The mod meta data from About.xml of the mod.
 */
export interface ModMetaData {
  /**
   * Path to directory of the mod.
   */
  readonly path: string;

  /**
   * Folder name of the mod.
   */
  readonly id: string;

  /**
   * Steam workshop publish file id of the mod.
   */
  readonly workshopId?: number;

  /**
   * Meta: name from About.xml
   */
  readonly name: string;

  /**
   * Meta: author from About.xml
   */
  readonly author: string;

  /**
   * Meta: url from About.xml
   */
  readonly url: string;
  /**
   * Meta: targetVersion from About.xml
   * @deprecated
   */
  readonly targetVersion: string;

  /**
   * Meta: supportedVersions from About.xml
   */
  readonly supportedVersions: readonly string[];

  /**
   * Meta: description from About.xml
   */
  readonly description: string;
}

export function defaultModMetaData(path: string): ModMetaData {
  const id = io.fileName(path);
  return {
    path,
    id,
    name: id,
    author: 'Anonymous',
    url: '',
    description: 'No description provided.',
    targetVersion: 'Unknown',
    supportedVersions: [],
  };
}

export interface ModOutput {
  /**
   * Path to the directory of the output
   */
  readonly path: string;

  /**
   * Path to the preview image of the mod.
   */
  readonly previewImage?: string;

  readonly pathAbout: string;

  readonly pathAssemblies: string;

  readonly pathDefs: string;

  readonly pathLanguages: string;

  readonly pathPatches: string;

  readonly pathTextures: string;

  readonly pathLanguage: (language: string) => string;
  readonly pathBackstories: (language: string) => string;
  readonly pathDefInjected: (language: string) => string;
  readonly pathKeyed: (language: string) => string;
  readonly pathStrings: (language: string) => string;
}

export class Mod implements ModOutput {
  public readonly meta: ModMetaData;

  /**
   * Path to directory of the mod.
   */
  public get path(): string {
    return this.meta.path;
  }

  /**
   * Folder name of the mod.
   */
  public get id(): string {
    return this.meta.id;
  }

  /**
   * Steam workshop publish file id of the mod.
   */
  public get workshopId(): number | undefined {
    return this.meta.workshopId;
  }

  /**
   * Path to the preview image of the mod.
   */
  public readonly previewImage?: string;

  public readonly pathAbout: string;

  public readonly pathAssemblies: string;

  public readonly pathDefs: string;

  public readonly pathLanguages: string;

  public readonly pathPatches: string;

  public readonly pathTextures: string;

  private constructor(pathRoot: string, modeMetaData: ModMetaData) {
    this.meta = modeMetaData;

    this.previewImage = io.join(pathRoot, FOLDER_NAME_ABOUT, FILE_NAME_PREVIEW);
    this.pathAbout = io.join(pathRoot, FOLDER_NAME_ABOUT);
    this.pathAssemblies = io.join(pathRoot, FOLDER_NAME_ASSEMBLIES);
    this.pathDefs = io.join(pathRoot, FOLDER_NAME_DEFS);
    this.pathLanguages = io.join(pathRoot, FOLDER_NAME_LANGUAGES);
    this.pathPatches = io.join(pathRoot, FOLDER_NAME_PATCHES);
    this.pathTextures = io.join(pathRoot, FOLDER_NAME_TEXTURES);
  }

  public pathLanguage(language: string): string {
    return io.join(this.pathLanguages, language);
  }

  public pathBackstories(language: string): string {
    return io.join(this.pathLanguages, language, FOLDER_NAME_BACKSTORIES);
  }

  public pathDefInjected(language: string): string {
    return io.join(this.pathLanguages, language, FOLDER_NAME_DEF_INJECTED);
  }

  public pathKeyed(language: string): string {
    return io.join(this.pathLanguages, language, FOLDER_NAME_KEYED);
  }

  public pathStrings(language: string): string {
    return io.join(this.pathLanguages, language, FOLDER_NAME_STRINGS);
  }

  public output(pathRoot: string): ModOutput {
    const previewImage = io.join(pathRoot, FOLDER_NAME_ABOUT, FILE_NAME_PREVIEW);
    const pathAbout = io.join(pathRoot, FOLDER_NAME_ABOUT);
    const pathAssemblies = io.join(pathRoot, FOLDER_NAME_ASSEMBLIES);
    const pathDefs = io.join(pathRoot, FOLDER_NAME_DEFS);
    const pathLanguages = io.join(pathRoot, FOLDER_NAME_LANGUAGES);
    const pathPatches = io.join(pathRoot, FOLDER_NAME_PATCHES);
    const pathTextures = io.join(pathRoot, FOLDER_NAME_TEXTURES);

    return {
      path: pathRoot,
      previewImage,
      pathAbout,
      pathAssemblies,
      pathDefs,
      pathLanguages,
      pathPatches,
      pathTextures,

      pathLanguage(language: string): string {
        return io.join(pathLanguages, language);
      },
      pathBackstories(language: string): string {
        return io.join(pathLanguages, language, FOLDER_NAME_BACKSTORIES);
      },
      pathDefInjected(language: string): string {
        return io.join(pathLanguages, language, FOLDER_NAME_DEF_INJECTED);
      },
      pathKeyed(language: string): string {
        return io.join(pathLanguages, language, FOLDER_NAME_KEYED);
      },
      pathStrings(language: string): string {
        return io.join(pathLanguages, language, FOLDER_NAME_STRINGS);
      },
    };
  }

  public static async load(path: string): Promise<Mod> {
    const pathAbout = io.join(path, FOLDER_NAME_ABOUT);
    const pathAboutXML = io.join(pathAbout, FILE_NAME_ABOUT);
    const pathPublishFileId = io.join(pathAbout, FILE_NAME_PUBLISHED_FILE_ID);

    const [meta, publishFileId] = await Promise.all([
      io.fileExists(pathAboutXML).then(
        async (exists): Promise<ModMetaData> => {
          const metaData: ModMetaData = defaultModMetaData(path);

          if (exists) {
            /* eslint-disable @typescript-eslint/no-explicit-any */
            try {
              const root = await loadXML(pathAboutXML);
              const excludes: (keyof ModMetaData)[] = ['path', 'id', 'workshopId'];
              Object.entries(metaData).forEach(([key, value]) => {
                if ((excludes as string[]).includes(key)) {
                  return;
                }
                const isArray = Array.isArray(value);
                const element = root.elements.find(c => c.name === key);
                if (!element) {
                  return;
                }
                if (isArray) {
                  (metaData as any)[key] = element.elements
                    .map(li => li.value.trim())
                    .filter(v => !!v);
                  return;
                }
                if (element.value.trim()) {
                  (metaData as any)[key] = element.value.trim();
                }
              });
            } catch (e) {
              const error = e as Error;
              (metaData as any).description = `Error:\n${error.message}\n${error.stack}`;
            }
            /* eslint-enable @typescript-eslint/no-explicit-any */
          }
          return metaData;
        },
      ),

      io.fileExists(pathPublishFileId).then(async exists => {
        if (exists) {
          const text = (await io.read(pathPublishFileId)).trim();
          const value = Number.parseInt(text, 10);
          if (!Number.isNaN(value) && value > 0) {
            return value;
          }
        }
        return undefined;
      }),
    ]);

    const mod = new Mod(path, meta);

    return mod;
  }
}
