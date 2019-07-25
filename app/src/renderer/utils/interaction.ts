import { remote } from 'electron';

/**
 * Select a directory by dialog.
 */
export async function selectDirectoryDialog(): Promise<string | undefined> {
  return new Promise<string | undefined>((resolve, reject) => {
    remote.dialog.showOpenDialog(
      {
        properties: ['openDirectory'],
      },
      ([path]: string[] = []) => {
        resolve(path);
      },
    );
  });
}
