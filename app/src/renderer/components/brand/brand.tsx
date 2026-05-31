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
 * Component: Brand
 */
@Component
export class RwBrand extends Vue {
  private render(h: CreateElement): VNode {
    return (
      <span staticClass="rw-brand">
        <rw-logo staticClass="rw-brand_logo" />
        {this.$slots.default || 'RimWorld'}
      </span>
    );
  }
}
