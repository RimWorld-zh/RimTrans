import { pth, fse, globby, ExtractorEventEmitter } from '@rimtrans/extractor';
import { IPC_NAMESPACE_TRANSLATOR_PROJECT } from '../../utils/constants';
import { createIpc } from '../../utils/ipc';
import { States } from '../../utils/states';
import { createSlaverMain } from '../../utils/slaver';
import { TranslationExtractorSlaver } from './extractor-slaver';

export function initExtractHandler(states: States): void {
  const { paths } = states;
}
