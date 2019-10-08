export interface FSWatchSlaver<TWatch, TSearch> {
  error: [never, Error];

  // watch
  add: [[string, TWatch], [string, TWatch]];
  addDir: [string, string];
  read: [string, [string, TWatch]];
  change: [[string, TWatch], [string, TWatch]];
  unlink: [string, string];
  unlinkDir: [string, string];

  // search
  search: [undefined, Record<string, TSearch>];
}

export type FSWatchIpcTypeMap<TWatch, TSearch> = FSWatchSlaver<TWatch, TSearch>;
