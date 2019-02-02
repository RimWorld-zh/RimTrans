// tslint:disable:no-any no-unsafe-any;
import compressing from 'compressing';

/**
 * Unzip a .zip file to a directory
 */
export async function unzip(src: string, dest: string): Promise<void> {
  return compressing.zip.uncompress(src, dest);
}

export async function zip(src: string, dest: string): Promise<void> {
  return compressing.zip.compressDir(src, dest);
}
