import fs from 'fs';

/**
 * Show all Def types.
 */
function allDefs(): void {
  fs.readFileSync('./scripts/Defs.txt', 'utf-8')
    .split('\n')
    .filter(t => t)
    .sort()
    .forEach(defType => console.log(defType));
}

allDefs();
