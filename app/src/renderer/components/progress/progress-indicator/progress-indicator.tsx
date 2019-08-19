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
import { when } from '../../base';

/**
 * Component: Progress Indicator
 */
@Component
export class RwProgressIndicator extends Vue {
  @Prop(Boolean)
  public readonly hidden?: boolean;

  @Prop({ type: Number, validator: (v: number): boolean => v >= 0 && v <= 1 })
  public readonly percentComplete?: number;

  private render(h: CreateElement): VNode {
    const { hidden, percentComplete } = this;

    const classes = when({ hidden });

    const hasPercentComplete = typeof percentComplete === 'number';
    const percent = Math.floor((percentComplete as number) * 100);
    const barStyle =
      (hasPercentComplete && {
        width: `${percent}%`,
      }) ||
      undefined;
    const barAttrs = (hasPercentComplete && {
      role: 'progressbar',
      'aria-valuemin': 0,
      'aria-valuemax': 100,
      'aria-valuenow': percent,
    }) || { role: 'progressbar' };

    const barClasses = when({ animation: !hasPercentComplete });

    return (
      <div staticClass="rw-progress-indicator" class={classes}>
        {h('div', {
          staticClass: 'rw-progress-indicator_bar',
          class: barClasses,
          style: barStyle,
          attrs: barAttrs,
        })}
      </div>
    );
  }
}
