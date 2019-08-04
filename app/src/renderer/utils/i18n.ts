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

declare module 'vue/types/vue' {
  interface Vue {
    readonly $i18n: I18n;
  }
}

declare module 'vue/types/options' {
  interface ComponentOptions<V extends Vue> {
    i18n?: I18n;
  }
}

let $$Vue: typeof Vue | undefined;

/**
 * Plugin I18n
 */
@Component
export class I18n extends Vue {
  public static install($Vue: typeof Vue): void {
    if ($$Vue && $$Vue === $Vue) {
      return;
    }
    $$Vue = $Vue;

    Object.defineProperty(Vue.prototype, '$i18n', {
      configurable: false,
      enumerable: false,
      get(this: Vue): I18n {
        return this.$root.$options.i18n as I18n;
      },
    });
  }

  public dict: TranslationDictionary = getLanguageDictByID('English');

  private lastLanguage?: string;

  @Watch('$states.settings.language', { immediate: true })
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
