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
import { Settings } from '@src/main';

/**
 * Component: Settings
 */
@Component
export default class VSettings extends Vue {
  private render(h: CreateElement): VNode {
    return (
      <div staticClass="v-settings">
        <div staticClass="v-settings_side-bar" />
        <div staticClass="v-settings_form">
          <pre>{JSON.stringify(this.$states.settings, undefined, '  ')}</pre>
        </div>
      </div>
    );
  }
}
