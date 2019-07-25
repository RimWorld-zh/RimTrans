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
  private render(h: CreateElement): VNode {
    const { language: settingsLanguage } = this.$states.settings;
    const stripOffset =
      languageInfos.findIndex(info => info.languageID === settingsLanguage) + 1;

    return (
      <div staticClass="v-settings-language">
        <rw-button
          key="auto"
          staticClass="v-settings-language_option"
          class={when({ active: settingsLanguage === 'auto' })}
          skin="flat"
          onClick={() => {
            this.$states.settings.language = 'auto';
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
