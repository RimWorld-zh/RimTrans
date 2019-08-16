export interface TranslatorProject {
  mods: string[];
}

export interface TranslatorProjectMod {
  path: string;
  output: boolean;
  outputPath: string;
}
