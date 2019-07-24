import Vue, { CreateElement, VNode } from 'vue';
import {
  Component,
  Emit,
  Inject,
  Model,
  Prop,
  Provide,
  Watch,
} from 'vue-property-decorator';
import { languageInfos } from '@rimtrans/i18n';
import { when } from '@src/renderer/components/base';

/**
 * Component: Settings Languages
 */
@Component
export default class VSettingsLanguage extends Vue {
  private stripOffset: number =
    languageInfos.findIndex(info => info.languageID === this.$states.settings.language) +
    1;

  private render(h: CreateElement): VNode {
    const { stripOffset } = this;
    const { language: settingsLanguage } = this.$states.settings;

    return (
      <div staticClass="v-settings-language">
        <rw-button
          key="auto"
          staticClass="v-settings-language_option"
          class={when({ active: settingsLanguage === 'auto' })}
          skin="flat"
          onClick={() => {
            this.$states.settings.language = 'auto';
            this.$i18n.languageID = 'auto';
            this.stripOffset = 0;
          }}
        >
          <span staticClass="v-settings-language_label is-primary">Auto</span>
          <span staticClass="v-settings-language_label">Current OS Language</span>
        </rw-button>
        {languageInfos.map(
          (
            {
              translators,
              languageID,
              languageCodes,
              languageNameNative,
              languageNameEnglish,
              progress,
            },
            index,
          ) => (
            <rw-button
              key={languageID}
              staticClass="v-settings-language_option"
              class={when({ active: settingsLanguage === languageID })}
              skin="flat"
              onClick={() => {
                this.$states.settings.language = languageID;
                this.$i18n.languageID = languageID;
                this.stripOffset = index + 1;
              }}
            >
              <span staticClass="v-settings-language_label is-primary">
                {languageNameNative}
              </span>
              <span staticClass="v-settings-language_label">{languageNameEnglish}</span>
              <span staticClass="v-settings-language_label">
                {Math.floor((progress as number) * 100)}%
              </span>
              <span staticClass="v-settings-language_label">
                {translators.join(', ')}
              </span>
            </rw-button>
          ),
        )}
        <rw-strip-v
          key="strip"
          staticClass="v-settings-language_strip"
          offset={stripOffset}
        />
      </div>
    );
  }
}
