import { Extractor, ExtractorConfig } from '@rimtrans/extractor';
import { createSlaverSub } from '../../utils/slaver';

export interface TranslationExtractorSlaver {
  extract: [ExtractorConfig, never];
}

const slaver = createSlaverSub<TranslationExtractorSlaver>();
const extractor = new Extractor(process);

slaver.addListener('extract', async config => {
  await extractor.extract(config);
});
