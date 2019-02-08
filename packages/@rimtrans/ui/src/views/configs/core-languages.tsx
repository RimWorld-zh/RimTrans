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
import { Languages } from '@rimtrans/service';
import worker from '@rimtrans/worker';

const language = 'language';
const latestUpdate = 'latest_update';
const update = 'update';
const updateAll = 'update_all';

/**
 * Component: CoreLanguages
 */
@Component
export class VCoreLanguages extends Vue {
  private timestamp: number = 0;
  private languages: [string, string][] = [];

  private updating: boolean = false;

  private async onUpdateAll(event: MouseEvent): Promise<void> {
    if (this.updating) {
      return;
    }
    this.updating = true;

    this.updating = false;
  }

  private async load(): Promise<void> {
    //
  }

  private async beforeMount(): Promise<void> {
    await this.load();
  }

  private render(h: CreateElement): VNode {
    return (
      <div staticClass="v-core-languages">
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

          {this.languages.map(([name, label]) => (
            <vd-flexbox align="center" gap="small">
              <vd-flexbox flex="none" />
              <vd-flexbox>
                {name}
                {name !== label && ` (${label})`}
              </vd-flexbox>
            </vd-flexbox>
          ))}
        </vd-flexbox>
      </div>
    );
  }
}
