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
import { prop, when } from '../../base';

/**
 * Component: Toggle
 */
@Component({ inheritAttrs: false })
export class RwToggle<T extends boolean | string | number> extends Vue {
  @Model('change', { type: [Boolean, String, Number], required: true })
  public readonly value!: T;

  @Prop({ type: Boolean, default: false })
  public readonly disabled!: boolean;

  @Prop(String)
  public readonly label?: string;

  @Prop({ type: Boolean, default: false })
  public readonly inlineLabel!: boolean;

  @Prop({ type: [Boolean, String, Number], default: true })
  public readonly valueOn!: T;

  @Prop({ type: [Boolean, String, Number], default: false })
  public readonly valueOff!: T;

  @Prop(String)
  public readonly textOn?: string;

  @Prop(String)
  public readonly textOff?: string;

  private render(h: CreateElement): VNode {
    const {
      $attrs,
      $listeners,
      value,
      disabled,
      label,
      inlineLabel,
      valueOn,
      valueOff,
      textOn,
      textOff,
    } = this;

    const checked = value === valueOn;

    const classes = when({ checked, disabled, 'inline-label': inlineLabel });

    const attrs = {
      ...$attrs,
      type: 'button',
      disabled,
      role: 'switch',
      'aria-checked': `${checked}`,
    };

    const onClick = (event: MouseEvent): void => {
      console.log(checked, valueOff, valueOn);
      this.$emit('change', checked ? valueOff : valueOn);
    };
    const on = {
      ...$listeners,
      change: onClick,
      click: onClick,
    };

    return (
      <div staticClass="rw-toggle" class={classes}>
        {!!label && <span staticClass="rw-toggle_label">{label}</span>}
        <label staticClass="rw-toggle_container">
          {h(
            'button',
            {
              staticClass: 'rw-toggle_button',
              attrs,
              on,
            },
            [h('i', { staticClass: 'rw-toggle_circle' })],
          )}
          {checked
            ? !!textOn && <span staticClass="rw-toggle_text">{textOn}</span>
            : !!textOff && <span staticClass="rw-toggle_text">{textOff}</span>}
        </label>
      </div>
    );
  }
}
