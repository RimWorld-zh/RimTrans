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
 * View: Translator Project
 */
@Component
export default class VTranslatorProject extends Vue {
  private render(h: CreateElement): VNode {
    return <div staticClass="v-translator-project">{this.$slots.default}</div>;
  }
}
