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
import { when, GOLDEN_RATIO } from '../../base';

/**
 * Component: AspectRatio
 */
@Component
export class RwAspectRatio extends Vue {
  /**
   * The aspect ratio to attempt to use.
   * The aspect ratio is expressed as a ratio of width to height.
   * For example, a 16:9 width:height aspect ratio would have a value of 16.0/9.0.
   */
  @Prop(Number)
  public readonly ratio?: number;

  @Prop({ type: Boolean, default: false })
  public readonly golden!: boolean;

  private render(h: CreateElement): VNode {
    const { ratio, golden } = this;

    const frameStyle =
      (golden && { paddingBottom: `${GOLDEN_RATIO * 100}%` }) ||
      (ratio && { paddingBottom: `${(1 / ratio) * 100}%` }) ||
      undefined;

    return (
      <div staticClass="rw-aspect-ratio" data-golden={golden} data-ratio={ratio}>
        <div staticClass="rw-aspect-ratio_frame" style={frameStyle}>
          <div staticClass="rw-aspect-ratio_container">{this.$slots.default}</div>
        </div>
      </div>
    );
  }
}
