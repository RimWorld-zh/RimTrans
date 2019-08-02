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
 * Component: RadioButton
 */
@Component
export class RwRadioButton extends Vue {
  private render(h: CreateElement): VNode {
    return <div staticClass="rw-radio-button">{this.$slots.default}</div>;
  }
}
