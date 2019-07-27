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
 * Component: Translator
 */
@Component
export class VTranslator extends Vue {
  private render(h: CreateElement): VNode {
    return <div staticClass="v-translator">{this.$slots.default}</div>;
  }
}
