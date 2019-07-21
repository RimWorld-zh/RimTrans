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
        <h2>Welcome to RimTrans</h2>
        <h3>Welcome to RimTrans</h3>
        <h4>Welcome to RimTrans</h4>
        <h5>Welcome to RimTrans</h5>
        <h6>Welcome to RimTrans</h6>
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
