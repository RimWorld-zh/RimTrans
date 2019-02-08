import Vue from 'vue';
import VueLocale, { Dict } from '@huiji/vue-locale';
Vue.use(VueLocale);

import { RadioData } from 'void-ui';

import { en } from './en';
import { zhCN } from './zh-CN';

const dicts: Record<string, Dict> = {};
const languages: RadioData[] = [];

[en, zhCN].forEach(info => {
  dicts[info.code] = info.dict;
  languages.push({
    label: `${info.label} (${info.name}) by ${info.translators.join(', ')}`,
    value: info.code,
  });
});

export { languages };

/**
 * Localization
 */
export const locale = new VueLocale({
  dicts,
});
