import Vue, {
  CreateElement,
  VNode,
  FunctionalComponentOptions,
  RenderContext,
} from 'vue';
import {
  Component,
  Emit,
  Inject,
  Model,
  Prop,
  Provide,
  Watch,
} from 'vue-property-decorator';
import * as MdiIconsMap from '@mdi/js';
import { when } from '../../base';

type MdiIconKey = keyof typeof MdiIconsMap;

/**
 * Component: Material Design Icon
 */
@Component
export class MdIcon extends Vue {
  @Prop({
    type: String,
    required: true,
    validator: (value: string) => {
      return !!MdiIconsMap[`mdi${value}` as MdiIconKey];
    },
  })
  public readonly icon!: string;

  @Prop({ type: Boolean, default: false })
  public readonly spin!: boolean;

  @Prop(Number)
  public readonly size?: number;

  @Prop(Number)
  public readonly rotate?: number;

  @Prop({ type: Boolean, default: false })
  public readonly flipH!: boolean;

  @Prop({ type: Boolean, default: false })
  public readonly flipV!: boolean;

  private render(h: CreateElement): VNode {
    const { icon, spin, size, rotate, flipH, flipV } = this;

    const classes = when({ spin, 'flip-h': flipH, 'flip-v': flipV });

    const styleSize = (typeof size === 'number' && `${size}px`) || null;
    const styleRotate = (typeof rotate === 'number' && `rotate(${rotate}deg)`) || null;

    return (
      <svg
        staticClass="md-icon"
        class={classes}
        style={{
          width: styleSize,
          height: styleSize,
          transform: styleRotate,
        }}
        viewBox="0 0 24 24"
        role="presentation"
        aria-hidden="true"
        onClick={(e: Event) => this.$emit('click', e)}
        data-icon={icon}
      >
        <path fill="currentColor" d={MdiIconsMap[`mdi${icon}` as MdiIconKey]} />
      </svg>
    );
  }
}
