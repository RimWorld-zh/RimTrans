import * as definition from './definition';
import * as injection from './injection';
import * as typeInfo from './type-package';

export interface ExtractorSolution {}

/**
 *
 * @param paths the array of paths to mod directories, order: `[core, ...mods]`.
 */
async function extract(paths: string[]): Promise<void> {
  Promise.all([
    definition.load(paths).then(maps => definition.resolveInheritance(maps)),
    typeInfo.load(paths),
  ]).then(([defMaps, classInfoMaps]) => injection.parse(defMaps, classInfoMaps));
}
