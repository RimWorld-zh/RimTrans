import Vue, { CreateElement, VNode } from 'vue';
import { VNodeChildren, ScopedSlotChildren } from 'vue/types/vnode';
import {
  Component,
  Emit,
  Inject,
  Model,
  Prop,
  Provide,
  Watch,
} from 'vue-property-decorator';
import { when } from '../../base';
import { ChoiceOption, ChoiceSlotProps } from '../input-models';

/**
 * Component: Checkbox
 */
@Component({ inheritAttrs: false })
export class RwCheckbox extends Vue {
  @Model('change', { type: Boolean, required: true })
  public readonly checked!: boolean;

  @Prop({ type: Boolean, default: false })
  public readonly disabled!: boolean;

  @Prop(String)
  public readonly label?: string;

  private render(h: CreateElement): VNode {
    const {
      $attrs,
      $listeners,
      checked,
      disabled,
      label,
      $slots: { default: defaultSlot },
    } = this;

    const classes = when({ checked });

    const attrs = {
      ...$attrs,
      type: 'checkbox',
      disabled,
      'aria-checked': `${checked}`,
    };

    const onChange = (event: Event): void => {
      this.$emit('change', !checked);
    };
    const on = {
      ...$listeners,
      change: onChange,
    };

    return (
      <label staticClass="rw-checkbox" class={classes}>
        {h('input', {
          staticClass: 'rw-checkbox_input',
          attrs,
          on,
        })}
        <i staticClass="rw-checkbox_button">
          <md-icon staticClass="rw-checkbox_check" icon="Check" />
        </i>
        {!!(defaultSlot || label) && (
          <span staticClass="rw-checkbox_label">{defaultSlot || label}</span>
        )}
      </label>
    );
  }
}

function wash<T extends string | number>(
  value: T[],
  options: ChoiceOption<T>[],
  index: number,
  checked: boolean,
): T[] {
  return [
    ...new Set(
      options
        .map(o => o.value)
        .filter((v, i) => {
          if (i === index) {
            return checked;
          }
          return value.includes(v);
        }),
    ),
  ];
}

@Component
export class RwMultiChoiceGroup<T extends string | number = string> extends Vue {
  @Model('change', { type: Array, required: true })
  public readonly value!: T[];

  @Prop({ type: Boolean, default: false })
  public readonly disabled!: boolean;

  @Prop(String)
  public readonly label?: string;

  @Prop({ type: Boolean, default: false })
  public readonly inlineLabel!: boolean;

  @Prop({ type: Array, required: true })
  public readonly options!: ChoiceOption<T>[];

  @Prop()
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  public readonly wrapperClass?: any;

  public readonly $scopedSlots!: {
    itemInner?: (props: ChoiceSlotProps<T>) => ScopedSlotChildren;
    itemOuter?: (props: ChoiceSlotProps<T>) => ScopedSlotChildren;
  };

  private render(h: CreateElement): VNode {
    const {
      $listeners,
      value: groupValue,
      disabled: groupDisabled,
      label: groupLabel,
      inlineLabel,
      options,
      wrapperClass,
      $scopedSlots: { itemInner = () => undefined, itemOuter = () => undefined },
    } = this;

    const classes = when({ disabled: groupDisabled, 'inline-label': inlineLabel });

    const change = (checked: boolean, index: number): void => {
      this.$emit('change', wash(groupValue, options, index, checked));
    };

    const renderItem: (option: ChoiceOption<T>, index: number) => VNodeChildren = (
      { value, label, disabled },
      index,
    ) => {
      const checked = groupValue.includes(value);
      const slotProps: ChoiceSlotProps<T> = {
        value,
        label,
        disabled,
        index,
        checked,
      };
      return [
        h(
          'rw-checkbox',
          {
            props: { checked, label, disabled: groupDisabled || disabled },
            on: { change: (v: boolean) => change(v, index) },
          },
          itemInner(slotProps),
        ),
        itemOuter(slotProps),
      ];
    };

    return (
      <div staticClass="rw-multi-choice-group" class={classes} role="group">
        {!!groupLabel && (
          <span key="label" staticClass="rw-multi-choice-group">
            {groupLabel}
          </span>
        )}
        <div
          key="wrapper"
          staticClass="rw-multi-choice-group_wrapper"
          class={wrapperClass}
        >
          {options.map(renderItem)}
        </div>
      </div>
    );
  }
}
