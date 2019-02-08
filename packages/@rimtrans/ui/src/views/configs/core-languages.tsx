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
import dateformat from 'dateformat';
import { wsc, LanguageCollection, LanguageData } from '@rimtrans/service';
import worker from '@rimtrans/worker';

const language = 'language';
const latestUpdate = 'latest_update';
const update = 'update';
const updateAll = 'update_all';

interface LanguageItem extends Pick<LanguageData, 'name' | 'internal' | 'status'> {
  label?: string;
}

/**
 * Component: ConfigsCoreLanguages
 */
@Component
export class VConfigsCoreLanguages extends Vue {
  private timestamp: number = 0;
  private items: LanguageItem[] = [];

  private updating: boolean = false;

  private async onUpdateAll(event: MouseEvent): Promise<void> {
    wsc.send('languageCollection', 'update');
  }

  private onLanguageCollection(data?: LanguageCollection): void {
    if (data) {
      this.timestamp = data.timestamp;
      this.items = data.items.map<LanguageItem>(
        ({ name, internal, status, info, friendly }) => {
          return {
            name,
            label: info ? worker.languageInfo(info).friendlyNameNative : friendly,
            internal,
            status,
          };
        },
      );
    }
  }

  private mounted(): void {
    wsc.addListener('languageCollection', this.onLanguageCollection);
    wsc.send('languageCollection');
  }

  private beforeDestroy(): void {
    wsc.removeListener('languageCollection', this.onLanguageCollection);
  }

  private render(h: CreateElement): VNode {
    return (
      <div staticClass="v-configs_core-languages">
        <vd-flexbox direction="column" align="stretch" gap="small">
          <vd-flexbox justify="space-between" align="center">
            <vd-flexbox flex="none">
              {this.$locale.dict[latestUpdate] || latestUpdate}
              {': '}
              {this.timestamp ? dateformat(this.timestamp, 'yyyy-mm-dd HH:MM:ss') : 'N/A'}
            </vd-flexbox>
            <vd-flexbox flex="none">
              <vd-button onClick={this.onUpdateAll} loading={this.updating}>
                {this.$locale.dict[updateAll] || updateAll}
              </vd-button>
            </vd-flexbox>
          </vd-flexbox>

          <vd-flexbox />

          {this.items.map(({ name, label, internal, status }) => (
            <vd-flexbox align="center" gap="small">
              <vd-flexbox flex="none">
                <fa-icon
                  staticClass="v-configs_lang-icon"
                  class={(!internal && 'is-external') || `is-${status}`}
                  icon={
                    (!internal && ['fas', 'folder']) ||
                    (status === 'success' && ['fas', 'check']) ||
                    (status === 'pending' && ['fas', 'circle-notch']) || ['fas', 'times']
                  }
                  spin={status === 'pending'}
                />
              </vd-flexbox>
              <vd-flexbox>
                {name}
                {label && label !== name && ` (${label})`}
              </vd-flexbox>
            </vd-flexbox>
          ))}
        </vd-flexbox>
      </div>
    );
  }
}
