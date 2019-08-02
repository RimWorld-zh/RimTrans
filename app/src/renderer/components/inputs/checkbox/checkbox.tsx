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
 * Component: Checkbox
 */
@Component
export class RwCheckbox<T extends string | number> extends Vue {
  @Model('change', { type: [String, Number], required: true })
  public readonly value!: T;

  private render(h: CreateElement): VNode {
    return <div staticClass="rw-checkbox">{this.$slots.default}</div>;
  }
}
