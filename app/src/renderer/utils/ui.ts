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
import { ColorTheme } from '@src/renderer/components/base';
import { States } from './states';

/**
 * Component State UI
 */
@Component
export class StateUi extends Vue {
  public readonly $parent!: States;

  @Watch('$parent.settings.theme', { immediate: true })
  private watchTheme(theme: ColorTheme): void {
    const {
      documentElement: { classList },
    } = document;
    classList.forEach(cls => {
      if (cls.startsWith('rw-theme-')) {
        classList.remove(cls);
      }
    });
    classList.add(`rw-theme-${theme}`);
  }
}
