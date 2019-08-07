import { remote } from 'electron';

/**
 * Select a directory by dialog.
 */
export async function selectDirectoryDialog(): Promise<string | undefined> {
  const { filePaths } = await remote.dialog.showOpenDialog(remote.getCurrentWindow(), {
    properties: ['openDirectory'],
  });

  return (filePaths && filePaths.length > 0 && filePaths[0]) || undefined;
}
