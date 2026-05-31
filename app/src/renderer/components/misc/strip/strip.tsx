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
import { prop, when, CSSStyle, Color, ButtonSize } from '../../base';

/**
 * Component: Strip
 */
@Component
export class RwStripV extends Vue {
  @Prop(Number)
  public readonly offset?: number;

  @Prop(String)
  public readonly size?: ButtonSize;

  @Prop({ type: String, default: 'primary' })
  public readonly color!: Color;

  private render(h: CreateElement): VNode {
    const { offset, size, color } = this;

    const style: CSSStyle | undefined =
      (offset && {
        transform: `translateY(${offset * 100}%)`,
      }) ||
      undefined;

    const classes = prop('rw', { size, color });

    return (
      <i staticClass="rw-strip-v" class={classes} style={style}>
        <i staticClass="rw-strip-v_rect" />
      </i>
    );
  }
}
