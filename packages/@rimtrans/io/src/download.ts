import fs from 'fs';
import pth from 'path';
import https from 'https';
import mkdirp from 'mkdirp';

/**
 * Download a file from the internet
 * @param url the url of the file to download
 * @param filename the path to save the file
 */
export async function download(url: string, filename: string): Promise<void> {
  return new Promise<void>((resolve, reject) => {
    const dir = pth.dirname(filename);
    if (!fs.existsSync(dir) || !fs.lstatSync(dir).isDirectory()) {
      mkdirp.sync(dir);
    }

    const file = fs.createWriteStream(filename);
    https.get(url, response => {
      if (
        response.statusCode &&
        300 < response.statusCode &&
        response.statusCode < 400 &&
        response.headers.location
      ) {
        download(response.headers.location, filename).then(resolve, reject);
      } else if (response.statusCode && response.statusCode >= 400) {
        reject(`${response.statusCode} ${response.statusMessage}`);
      } else {
        response
          .on('data', data => file.write(data))
          .on('end', () => {
            file.end(resolve);
          })
          .on('error', reject);
      }
    });
  });
}
