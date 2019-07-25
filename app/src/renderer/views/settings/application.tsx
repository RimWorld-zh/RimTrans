import { remote } from 'electron';
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
import { selectDirectoryDialog } from '@src/renderer/utils';

/**
 * Component: SettingsApplication
 */
@Component
export default class VSettingsApplication extends Vue {
  private async onDirectoryRimWorldExplorer(event: MouseEvent): Promise<void> {
    const path = await selectDirectoryDialog();
    if (path) {
      this.$states.settings.directoryRimWorld = path;
    }
  }

  private async onDirectoryWorkshopExplorer(event: MouseEvent): Promise<void> {
    const path = await selectDirectoryDialog();
    if (path) {
      this.$states.settings.directoryWorkshop = path;
    }
  }

  private render(h: CreateElement): VNode {
    return (
      <div staticClass="v-settings-application">
        <rw-form>
          <rw-form-label key="dir-rw-label">RimWorld Directory</rw-form-label>
          <rw-form-field-group key="dir-rw" staticClass="v-settings-application_group">
            <rw-text-field vModel={this.$states.settings.directoryRimWorld} />
            <rw-button color="primary" onClick={this.onDirectoryRimWorldExplorer}>
              Explorer
            </rw-button>
          </rw-form-field-group>

          <rw-form-label key="dir-ws-label">Workshop Mods Directory</rw-form-label>
          <rw-form-field-group key="dir-ws" staticClass="v-settings-application_group">
            <rw-text-field vModel={this.$states.settings.directoryWorkshop} />
            <rw-button color="primary" onClick={this.onDirectoryWorkshopExplorer}>
              Explorer
            </rw-button>
          </rw-form-field-group>
        </rw-form>
      </div>
    );
  }
}
