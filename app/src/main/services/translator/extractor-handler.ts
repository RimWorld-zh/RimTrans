import { ExtractorEventEmitter } from '@rimtrans/extractor';
import { States } from '../../utils/states';
import { createSlaverMain } from '../../utils/slaver';
import { TranslationExtractorSlaver } from './extractor-slaver';

export function initHandler(states: States): void {
  const { ipc } = states;
}
