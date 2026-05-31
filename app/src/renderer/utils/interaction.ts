import { remote, shell } from 'electron';

async function selectDirectoryDialog(): Promise<string | undefined> {
  const { filePaths } = await remote.dialog.showOpenDialog(remote.getCurrentWindow(), {
    properties: ['openDirectory'],
  });

  return (filePaths && filePaths.length > 0 && filePaths[0]) || undefined;
}

function openItem(fullPath: string): boolean {
  return shell.openItem(fullPath);
}

function showItemInFolder(fullPath: string): void {
  return shell.showItemInFolder(fullPath);
}

/**
 * The util interaction is the wrapper of election api.
 */
const interaction = {
  /**
   * Select a directory by dialog.
   */
  selectDirectoryDialog,

  /**
   * Open the given file in the desktop's default manner.
   */
  openItem,

  /**
   * Show the given file in a file manager. If possible, select the file.
   */
  showItemInFolder,
};

Object.freeze(interaction);

export { interaction };
