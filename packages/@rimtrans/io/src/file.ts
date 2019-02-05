import fs from 'fs';
import mkdirp from 'mkdirp';
import rimraf from 'rimraf';

/**
 * Load a text file.
 * @param file the path to the file
 * @param unify if true, remove the UTF-8 BOM and replace CRLF to LF
 */
export async function load(file: string, unify: boolean = false): Promise<string> {
  return new Promise<string>((resolve, reject) =>
    fs.readFile(file, { encoding: 'utf-8' }, (error, content) =>
      error
        ? reject(error)
        : resolve(
            unify ? content.replace(/^\ufeff/g, '').replace(/\r\n/g, '\n') : content,
          ),
    ),
  );
}

/**
 * Save a text file.
 * @param file path to save the file
 * @param content the text content
 */
export async function save(file: string, content: string): Promise<void> {
  return new Promise<void>((resolve, reject) =>
    fs.writeFile(file, content, { encoding: 'utf-8' }, error =>
      error ? reject(error) : resolve(),
    ),
  );
}

/**
 * Copy a file.
 * @param src the path to the source file
 * @param dest the path to the destination file
 */
export async function copy(src: string, dest: string): Promise<void> {
  return new Promise<void>((resolve, reject) =>
    fs.copyFile(src, dest, error => (error ? reject(error) : resolve())),
  );
}

/**
 * Create a directory.
 * @param dir the path to the directory
 */
export async function createDirectory(dir: string): Promise<void> {
  return new Promise<void>((resolve, reject) =>
    mkdirp(dir, error => (error ? reject(error) : resolve())),
  );
}

/**
 * Remove a file or a directory.
 * @param path the path to the file or the directory
 */
export async function remove(path: string): Promise<void> {
  return new Promise<void>((resolve, reject) =>
    rimraf(path, error => (error ? reject(error) : resolve())),
  );
}
