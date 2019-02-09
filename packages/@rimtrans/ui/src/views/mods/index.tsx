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
 * Component: Mods
 */
@Component
export class VMods extends Vue {
  private render(h: CreateElement): VNode {
    return <div staticClass="v-mods">{this.$slots.default}</div>;
  }
}
