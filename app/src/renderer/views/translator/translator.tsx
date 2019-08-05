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
import { selectModDialog } from '@src/renderer/services';

/**
 * Component: Translator
 */
@Component
export default class VTranslator extends Vue {
  private showDialog = true;

  private mods: string[] = [];

  private async onToggleDialog(event: MouseEvent): Promise<void> {
    const mods = await selectModDialog(this);
    if (mods) {
      this.mods = mods;
    }
  }

  private render(h: CreateElement): VNode {
    return (
      <div staticClass="v-translator">
        <div>
          <rw-button color="primary" onClick={this.onToggleDialog}>
            toggle
          </rw-button>
        </div>
        <h1>translator</h1>
        <ul>
          {this.mods.map(mod => (
            <li>{mod}</li>
          ))}
        </ul>
      </div>
    );
  }
}
