export enum ContentSource {
  // basic
  Undefined,
  LocalFolder,
  SteamWorkshop,

  // extended
  Remote,
}

/**
 * Mod info.
 */
export interface ModMetaData {
  // basic

  name: string;
  author: string;
  url: string;
  targetVersion: string;
  description: string;

  // extended

  previewImagePath: string;

  /**
   * The location of the mod.
   */
  source: ContentSource;

  /**
   * steam workshop id
   */
  publishedFileId: number;
}
