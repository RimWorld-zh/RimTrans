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

interface NavItem {}

const navList = [];

/**
 * Component: Welcome
 */
@Component
export default class VWelcome extends Vue {
  private render(h: CreateElement): VNode {
    return (
      <div staticClass="v-welcome">
        <div staticClass="v-welcome_wrapper" />
      </div>
    );
  }
}
