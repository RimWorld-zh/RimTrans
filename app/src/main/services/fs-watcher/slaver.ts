import chokidar, { WatchOptions, watch } from 'chokidar';
import { pth, fse, globby } from '@rimtrans/extractor';
import { SlaverSub, createSlaverSub } from '../../utils/slaver';
import { FSWatchSlaver } from './models';

export interface FSWatchSlaverOptions<TWatch, TSearch> {
  directory: string;

  // watch
  watchOptions: WatchOptions;
  save: (path: string, data: TWatch) => Promise<void>;
  load: (path: string) => Promise<TWatch>;

  // search
  searchGlob: string[];
  searchGlobbyOptions: globby.GlobbyOptions;
  searchResolver: (path: string) => Promise<TSearch>;
}

export function setupFSWatchSlaver<TWatch = string, TSearch = string>(
  options: FSWatchSlaverOptions<TWatch, TSearch>,
): SlaverSub<FSWatchSlaver<TWatch, TSearch>> {
  const {
    directory,
    watchOptions,
    save,
    load,
    searchGlob,
    searchGlobbyOptions,
    searchResolver,
  } = options;

  const slaver = createSlaverSub<FSWatchSlaver<TWatch, TSearch>>();

  const watcher = chokidar.watch(directory, watchOptions);

  watcher.on('error', error => slaver.send('error', error));

  slaver.addListener('add', async payload => save(...payload));
  watcher.on('add', path => load(path).then(data => slaver.send('add', [path, data])));

  slaver.addListener('addDir', async path => fse.mkdirp(path));
  watcher.on('addDir', path => slaver.send('addDir', path));

  slaver.addListener('read', async path => {
    const data = await load(path);
    slaver.send('read', [path, data]);
  });

  slaver.addListener('change', async payload => save(...payload));
  watcher.on('change', path =>
    load(path).then(data => slaver.send('change', [path, data])),
  );

  slaver.addListener('unlink', async path => fse.remove(path));
  watcher.on('unlink', path => slaver.send('unlink', path));

  slaver.addListener('unlinkDir', async path => fse.remove(path));
  watcher.on('unlinkDir', path => slaver.send('unlinkDir', path));

  slaver.addListener('search', async () => {
    const result: Record<string, TSearch> = {};
    const cwd = searchGlobbyOptions.cwd || directory;

    if (!(await fse.pathExists(cwd))) {
      slaver.send('search', result);
      return;
    }

    const files = await globby(searchGlob, {
      ...searchGlobbyOptions,
      cwd,
    });
    await Promise.all(
      files.map(async filename => {
        const path = pth.join(cwd, filename);
        const data = await searchResolver(path);
        result[path] = data;
      }),
    );
    slaver.send('search', result);
  });

  return slaver;
}
