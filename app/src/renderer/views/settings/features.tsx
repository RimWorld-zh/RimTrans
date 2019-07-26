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
 * Component: Settings Feature
 */
@Component
export default class VSettingsFeature extends Vue {
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
    const {
      $i18n: {
        dict: {
          settingsExplorerDirectory,
          settingsDirectoryRimWorld,
          settingsDirectoryWorkshop,
        },
      },
    } = this;

    return (
      <div staticClass="v-settings-form">
        <rw-form>
          <rw-form-label key="dir-rw-label">{settingsDirectoryRimWorld}</rw-form-label>
          <rw-form-field-group key="dir-rw" staticClass="v-settings-form_directory-group">
            <rw-text-field v-model={this.$states.settings.directoryRimWorld} />
            <rw-button color="primary" onClick={this.onDirectoryRimWorldExplorer}>
              {settingsExplorerDirectory}
            </rw-button>
          </rw-form-field-group>

          <rw-form-label key="dir-ws-label">{settingsDirectoryWorkshop}</rw-form-label>
          <rw-form-field-group key="dir-ws" staticClass="v-settings-form_directory-group">
            <rw-text-field v-model={this.$states.settings.directoryWorkshop} />
            <rw-button color="primary" onClick={this.onDirectoryWorkshopExplorer}>
              {settingsExplorerDirectory}
            </rw-button>
          </rw-form-field-group>
        </rw-form>
      </div>
    );
  }
}
