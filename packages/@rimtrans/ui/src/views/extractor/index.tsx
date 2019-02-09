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
 * Component: Extractor
 */
@Component
export class VExtractor extends Vue {
  private render(h: CreateElement): VNode {
    return (
      <div staticClass="v-extractor">
        <vd-swimlane>
          <vd-container>
            <router-view />
          </vd-container>
        </vd-swimlane>
      </div>
    );
  }
}
