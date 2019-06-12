import jsdom from 'jsdom';
import { TypeInfoMap, getCoreTypeInfo } from '@rimtrans/type-info';
import { getCoreFiles } from '@rimtrans/rimworld-core';

export async function extract(): Promise<string> {
  await getCoreTypeInfo();
  await getCoreFiles();
  return 'extracted';
}
