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
import { ModMetaData } from '@rimtrans/extractor';

/**
 * Component: Translator
 */
@Component
export default class VTranslator extends Vue {
  private async onShowDialog(event: MouseEvent): Promise<void> {
    const value = await this.$rw_showDialog(
      {
        title: 'Dialog',
        content: 'This is a dialog',
        confirmLabel: 'Confirm',
        cancelLabel: 'Cancel',
      },
      this,
    );
  }

  private mods: Record<string, ModMetaData> = {};

  private async onLoadMods(event: Event): Promise<void> {
    this.mods = await this.$ipc.request('getModMetaData', 'workshop');
  }

  private render(h: CreateElement): VNode {
    const { mods } = this;

    return (
      <div staticClass="v-translator">
        <h1>translator</h1>
        <div>
          <rw-button onClick={this.onShowDialog}>select mod</rw-button>
        </div>
      </div>
    );
  }
}
