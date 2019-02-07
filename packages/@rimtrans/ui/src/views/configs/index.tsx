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
 * Component: Configs
 */
@Component
export class VConfigs extends Vue {
  private render(h: CreateElement): VNode {
    return <div staticClass="v-configs">configs</div>;
  }
}
