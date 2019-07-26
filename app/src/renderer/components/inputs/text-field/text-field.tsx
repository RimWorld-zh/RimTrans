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
 * Component: Text Field
 */
@Component({ inheritAttrs: false })
export class RwTextField extends Vue {
  @Model('input', { type: [String, Number], required: true })
  public readonly value!: string | number;

  @Prop({ type: Boolean, default: false })
  public readonly disabled!: boolean;

  @Prop(String)
  public readonly label?: string;

  @Prop({ type: Boolean, default: false })
  public readonly inlineLabel!: boolean;

  @Prop(String)
  public readonly prefix?: string;

  @Prop(String)
  public readonly suffix?: string;

  private render(h: CreateElement): VNode {
    const {
      $attrs,
      $listeners,
      $slots: { prefixIcon, suffixIcon },
      value,
      disabled,
      label,
      inlineLabel,
      prefix,
      suffix,
    } = this;

    const classes = when({ disabled, 'inline-label': inlineLabel });

    const attrs = {
      ...$attrs,
      type: 'text',
      value,
      disabled,
    };
    const on = {
      ...$listeners,
      input: (event: Event) => {
        this.$emit('input', (event.target as HTMLInputElement).value);
      },
    };

    return (
      <label staticClass="rw-text-field" class={classes}>
        {!!label && <span staticClass="rw-text-field_label">{label}</span>}
        <span staticClass="rw-text-field_container">
          {!!prefix && <span staticClass="rw-text-field_prefix">{prefix}</span>}
          {!!prefixIcon && (
            <span staticClass="rw-text-field_prefix-icon">{prefixIcon}</span>
          )}
          {h('input', { staticClass: 'rw-text-field_input', attrs, on })}
          {!!suffixIcon && (
            <span staticClass="rw-text-field_suffix-icon">{suffixIcon}</span>
          )}
          {!!suffix && <span staticClass="rw-text-field_suffix">{suffix}</span>}
        </span>
      </label>
    );
  }
}
