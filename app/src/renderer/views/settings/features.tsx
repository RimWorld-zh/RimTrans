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
import { interaction } from '@src/renderer/utils';

/**
 * Component: Settings Feature
 */
@Component
export default class VSettingsFeature extends Vue {
  private async onDirectoryRimWorldExplorer(event: MouseEvent): Promise<void> {
    const path = await interaction.selectDirectoryDialog();
    if (path) {
      this.$states.settings.directoryRimWorld = path;
    }
  }

  private async onDirectoryWorkshopExplorer(event: MouseEvent): Promise<void> {
    const path = await interaction.selectDirectoryDialog();
    if (path) {
      this.$states.settings.directoryWorkshop = path;
    }
  }

  private render(h: CreateElement): VNode {
    const {
      $states: {
        i18n: {
          file: { explore },
          settings: { directoryRimWorld, directoryWorkshop },
        },
      },
    } = this;

    return (
      <div staticClass="v-settings-form">
        <rw-form>
          <rw-form-label key="dir-rw-label">{directoryRimWorld}</rw-form-label>
          <rw-form-field-group key="dir-rw" staticClass="v-settings-form_directory-group">
            <rw-text-field v-model={this.$states.settings.directoryRimWorld}>
              <md-icon slot="prefixIcon" icon="FolderOutline" />
            </rw-text-field>
            <rw-button color="primary" onClick={this.onDirectoryRimWorldExplorer}>
              {explore}
            </rw-button>
          </rw-form-field-group>

          <rw-form-label key="dir-ws-label">{directoryWorkshop}</rw-form-label>
          <rw-form-field-group key="dir-ws" staticClass="v-settings-form_directory-group">
            <rw-text-field v-model={this.$states.settings.directoryWorkshop}>
              <md-icon slot="prefixIcon" icon="FolderOutline" />
            </rw-text-field>
            <rw-button color="primary" onClick={this.onDirectoryWorkshopExplorer}>
              {explore}
            </rw-button>
          </rw-form-field-group>
        </rw-form>
      </div>
    );
  }
}
