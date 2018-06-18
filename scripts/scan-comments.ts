/**
 * Scan all of comments in RimWorld Mods.
 */

import chalk from 'chalk';
import xmljs from 'xml-js';
import globby from 'globby';
import fs from 'fs';

function scanComments(): void {
  globby
    .sync(
      [
        '/mnt/d/Games/SteamLibrary/steamapps/common/RimWorld/Mods/Core/Defs',
        '/mnt/d/Games/SteamLibrary/steamapps/workshop/content/294100',
      ].map(p => `${p}/**/*.xml`),
    )
    .forEach(p =>
      fs.readFile(p, { encoding: 'utf-8' }, (err, data) => {
        if (err) {
          console.log(chalk.redBright(p));
          console.error(err);
        }
        try {
          const doc: xmljs.ElementCompact = xmljs.xml2js(data, { compact: true });
          if (doc.Defs && (doc.Defs as xmljs.ElementCompact)._comment) {
            const root: xmljs.ElementCompact = doc.Defs;

            let comments: string[] = [];

            if (typeof root._comment === 'string') {
              comments = [root._comment];
            } else if (Array.isArray(root._comment)) {
              comments = root._comment;
            }

            comments = comments.filter(c => (c.match(/\r?\n/g) || []).length === 0);

            // if (comments.length > 0) {
            //   console.log(chalk.greenBright(p));
            //   comments.forEach(c => console.log(c));
            // }
          }
        } catch (error) {
          console.log(chalk.redBright(p));
          console.log(error);
        }
      }),
    );
}

scanComments();
