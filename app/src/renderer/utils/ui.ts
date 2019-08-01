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

/**
 * Component States UI
 */
@Component
export class StatesUI extends Vue {
  @Watch('$states.settings.theme', { immediate: true })
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
