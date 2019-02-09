// tslint:disable:no-any no-unsafe-any
import fs from 'fs';
import pth from 'path';
import axios from 'axios';
import mkdirp from 'mkdirp';
import { Stream } from 'stream';

/**
 * Download a file from the internet
 * @param url the url of the file to download
 * @param filename the path to save the file
 * @param cb the callback function to get current and total file size (bytes) in process, note that total maybe NaN
 */
export async function download(
  url: string,
  filename: string,
  cb?: (current: number, total?: number) => any,
): Promise<void> {
  const dir = pth.dirname(filename);
  if (!fs.existsSync(dir) || !fs.lstatSync(dir).isDirectory()) {
    mkdirp.sync(dir);
  }

  const writer = fs.createWriteStream(filename);
  const response = await axios({
    url,
    method: 'GET',
    responseType: 'stream',
  });
  const stream = response.data as Stream;

  if (cb) {
    const total = parseInt(
      response.headers['content-length'] || response.headers['Content-Length'],
      10,
    );
    let current = 0;
    stream.on('data', chunk => {
      current += chunk.length as number;
      cb(current, total);
    });
  }
  stream.pipe(writer);

  return new Promise<void>((resolve, reject) => {
    writer.on('finish', resolve);
    writer.on('error', reject);
  });
}
