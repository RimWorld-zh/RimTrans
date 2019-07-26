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
import { Theme } from '@src/renderer/components/base';

/**
 * Component States UI
 */
@Component
export class StatesUI extends Vue {
  @Watch('$states.settings.theme', { immediate: true })
  private watchTheme(theme: Theme): void {
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
