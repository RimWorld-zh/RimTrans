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
  LanguageDictionary,
  LanguageData,
  languageInfos,
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

  private $languageID: string = 'auto';

  public get languageID(): string {
    return this.$languageID;
  }

  public set languageID(value: string) {
    this.select(value);
  }

  public dict: LanguageDictionary = getLanguageDictByID('English');

  public select(languageID: string): void {
    this.$languageID = languageID;
    let id = languageID;
    if (languageID.toLowerCase() === 'auto') {
      id = getLanguageIdByCode(remote.app.getLocale());
    }
    this.dict = getLanguageDictByID(id);
  }
}
