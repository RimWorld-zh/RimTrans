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
import { prop, when, SpinnerColor, SpinnerSize } from '../../base';

/**
 * Component: Spinner
 */
@Component
export class RwSpinner extends Vue {
  @Prop({ type: Boolean, default: false })
  public readonly hidden!: boolean;

  @Prop(String)
  public readonly size?: SpinnerSize;

  private render(h: CreateElement): VNode {
    const { hidden, size } = this;

    const classes = {
      ...prop('rw', { size }),
      ...when({ hidden }),
    };

    return (
      <i staticClass="rw-spinner" class={classes}>
        <i staticClass="rw-spinner_circle" />
      </i>
    );
  }
}
