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
 * Component: App
 */
@Component
export default class VApp extends Vue {
  private render(h: CreateElement): VNode {
    return (
      <div staticClass="v-app">
        <router-view />
      </div>
    );
  }
}
