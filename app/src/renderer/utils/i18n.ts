import { remote } from 'electron';
import Vue from 'vue';
import {
  Component,
  Emit,
  Inject,
  Model,
  Prop,
  Provide,
  Watch,
} from 'vue-property-decorator';
import {
  LanguageInfo,
  LanguageData,
  languageInfos,
  TranslationDictionary,
  getLanguageIdByCode,
  getLanguageDictByID,
} from '@rimtrans/i18n';
import { States } from './states';

/**
 * Plugin I18n
 */
@Component
export class StateI18n extends Vue {
  public readonly $parent!: States;

  public dict: TranslationDictionary = getLanguageDictByID('English');

  private lastLanguage?: string;

  @Watch('$parent.settings.language', { immediate: true })
  private select(language: string): void {
    let id = language;
    if (id.toLowerCase() === 'auto') {
      id = getLanguageIdByCode(remote.app.getLocale());
    }
    if (id !== this.lastLanguage) {
      this.dict = getLanguageDictByID(id);
      this.lastLanguage = id;
    }
  }
}
