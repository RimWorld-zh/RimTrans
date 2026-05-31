import { States } from '../utils/states';
import { initModMetaDataHandler } from './mod-meta-data';
import { initProjectHandler, initExtractHandler } from './translator';

export * from './mod-meta-data';
export * from './translator';

export function initServices(states: States): void {
  [initModMetaDataHandler, initProjectHandler, initExtractHandler].forEach(init =>
    init(states),
  );
}
