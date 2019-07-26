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

/**
 * Component: SettingsUI
 */
@Component
export default class VSettingsUI extends Vue {
  private render(h: CreateElement): VNode {
    return (
      <div staticClass="v-settings-form">
        <rw-form>
          <rw-form-label key="theme-label">Dark mode</rw-form-label>
          <rw-toggle
            key="theme"
            v-model={this.$states.settings.theme}
            value-on="dark"
            value-off="light"
          />
        </rw-form>
      </div>
    );
  }
}
