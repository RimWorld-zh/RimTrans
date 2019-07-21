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
 * Component: Settings Languages
 */
@Component
export default class VSettingsLanguages extends Vue {
  private render(h: CreateElement): VNode {
    return <div staticClass="v-settings-languages">{this.$slots.default}</div>;
  }
}
