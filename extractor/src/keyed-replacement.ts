export interface KeyedReplacement {
  path: string;
  origin: string;
  translation: string;
  duplicated?: boolean;
}

export interface KeyedReplacementMap {
  [fileName: string]: (string | KeyedReplacement)[];
}
