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
import { prop, when, ColorSemantic, Severity } from '../../base';

/**
 * Component: Text Field
 */
@Component({ inheritAttrs: false })
export class RwTextField extends Vue {
  @Model('input', { type: String, required: true })
  public readonly value!: string;

  @Prop({ type: Boolean, default: false })
  public readonly disabled!: boolean;

  @Prop({ type: Boolean, default: false })
  public readonly autofocus!: boolean;

  @Prop(String)
  public readonly label?: string;

  @Prop({ type: Boolean, default: false })
  public readonly inlineLabel!: boolean;

  @Prop(String)
  public readonly prefix?: string;

  @Prop(String)
  public readonly suffix?: string;

  @Prop([Function, RegExp])
  public readonly validator?: ((text: string | number) => Severity | boolean) | RegExp;

  public shouldValidate: boolean = false;

  private inputCount: number = 0;

  private render(h: CreateElement): VNode {
    const {
      $attrs,
      $listeners: { blur: rawBlur, ...on },
      $slots: { prefixIcon, suffixIcon },
      value,
      disabled,
      autofocus,
      label,
      inlineLabel,
      prefix,
      suffix,
      validator,
      shouldValidate,
    } = this;

    let status: Severity | ColorSemantic | boolean | undefined;
    if (validator && shouldValidate) {
      status =
        typeof validator === 'function'
          ? validator(value)
          : (validator as RegExp).test(value);
      switch (status as (Severity | boolean)) {
        case true:
        case 'success':
        case 'safe':
          status = 'success';
          break;

        case 'warning':
          // status = 'warning';
          break;

        case false:
        case 'danger':
        case 'error':
        case 'fatal':
          status = 'danger';
          break;

        default:
          status = undefined;
      }
    }

    const classes = {
      ...prop('rw', { status }),
      ...when({ disabled, 'inline-label': inlineLabel }),
    };

    const attrs = {
      ...$attrs,
      type: 'text',
      value,
      disabled,
      autofocus,
    };

    const input = (event: Event): void => {
      this.$emit('input', (event.target as HTMLInputElement).value);
    };
    let firstInput = (event: Event): void => {
      this.shouldValidate = true;
      input(event);
      firstInput = input;
    };

    const blur = (event: Event): void => {
      if (rawBlur) {
        if (Array.isArray(rawBlur)) {
          rawBlur.forEach(l => l(event));
        } else {
          rawBlur(event);
        }
      }
      this.shouldValidate = true;
    };

    return (
      <label staticClass="rw-text-field" class={classes}>
        {!!label && <span staticClass="rw-text-field_label">{label}</span>}
        <span staticClass="rw-text-field_container">
          {!!prefix && <span staticClass="rw-text-field_prefix">{prefix}</span>}
          {!!prefixIcon && (
            <span staticClass="rw-text-field_prefix-icon">{prefixIcon}</span>
          )}
          {h('input', {
            staticClass: 'rw-text-field_input',
            attrs,
            on: { ...on, blur, input: (e: Event) => firstInput(e) },
          })}
          {!!suffixIcon && (
            <span staticClass="rw-text-field_suffix-icon">{suffixIcon}</span>
          )}
          {!!suffix && <span staticClass="rw-text-field_suffix">{suffix}</span>}
        </span>
      </label>
    );
  }
}
