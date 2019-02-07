/**
 * Interface ModMetaData is the type define for mods' About.xml
 */
export interface ModMetaData {
  name: string;
  author: string;
  url: string;
  targetVersion: string;
  supportedVersions: string[];
  description: string;
}
