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
 * Component: PlaceHolder
 */
@Component
export default class VPlaceHolder extends Vue {
  private render(h: CreateElement): VNode {
    return (
      <div staticClass="v-place-holder">
        <rw-aspect-ratio golden>
          <header staticClass="v-place-holder_icon">🚧</header>
          <h1>Work in progress: {this.$route.name}</h1>
        </rw-aspect-ratio>
      </div>
    );
  }
}
