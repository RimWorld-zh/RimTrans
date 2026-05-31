import Vue, { CreateElement, RenderContext, VNode } from 'vue';
import {
  Component,
  Emit,
  Inject,
  Model,
  Prop,
  Provide,
  Watch,
} from 'vue-property-decorator';
import { prop, when, ButtonColor, ButtonSize, ButtonSkin, ButtonShape } from '../../base';

/**
 * Component: Button
 */
@Component
export class RwButton extends Vue {
  @Prop({ type: String, default: 'button' })
  public readonly tag!: string;

  @Prop({ type: Boolean, default: false })
  public readonly routerLink!: boolean;

  @Prop({ type: String, default: 'button' })
  public readonly type!: string;

  @Prop({ type: Boolean, default: false })
  public readonly disabled!: boolean;

  @Prop(String)
  public readonly color?: ButtonColor;

  @Prop(String)
  public readonly size?: ButtonSize;

  @Prop(String)
  public readonly skin?: ButtonSkin;

  @Prop(String)
  public readonly shape?: ButtonShape;

  @Prop(Number)
  public readonly depth?: number;

  private render(h: CreateElement): VNode {
    const {
      tag,
      routerLink,
      type,
      disabled,
      color,
      size,
      skin,
      shape,
      depth,
      $attrs: attrs,
      $listeners: on,
    } = this;

    const classes = {
      ...prop('rw', { color, size, skin, shape, depth }),
      ...when({ disabled }),
    };

    return h(
      (routerLink && 'router-link') || tag,
      {
        staticClass: 'rw-button',
        class: classes,
        attrs: {
          type,
          disabled,
          'aria-disabled': disabled,
        },
        props: (routerLink && { tag, ...attrs }) || undefined,
        on,
      },
      [h('div', { staticClass: 'rw-button_content' }, this.$slots.default)],
    );
  }
}
