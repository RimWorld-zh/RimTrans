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
 * Component: Welcome
 */
@Component
export default class VWelcome extends Vue {
  private render(h: CreateElement): VNode {
    return (
      <div staticClass="v-welcome">
        <h1>Welcome to RimTrans</h1>
        <ul>
          {Object.entries(this.$route.query).map(([key, value]) => (
            <li>
              {key}: {value}
            </li>
          ))}
        </ul>
      </div>
    );
  }
}
