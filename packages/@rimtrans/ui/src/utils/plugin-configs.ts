// tslint:disable:no-reserved-keywords
import Vue, { PluginObject } from 'vue';
import {
  Component,
  Emit,
  Inject,
  Model,
  Prop,
  Provide,
  Watch,
} from 'vue-property-decorator';
import { wsc, Configs, newConfigs } from '@rimtrans/service';

let $$Vue: typeof Vue | undefined;

declare module 'vue/types/vue' {
  interface Vue {
    $configs: Configs;
  }
}

/**
 * Component Configs
 */
@Component
export class PluginConfigs extends Vue {
  public configs: Configs = newConfigs();

  public reset(): void {
    this.configs = newConfigs();
  }

  private async setLanguage(): Promise<void> {
    await this.$locale.selectLanguage(
      this.configs.language === 'auto' ? navigator.language : this.configs.language,
    );
  }

  @Watch('configs', { deep: true })
  private async watchConfigs(): Promise<void> {
    await this.setLanguage();
    wsc.send('configs', this.configs);
  }

  private async onConfigs(data?: Configs): Promise<void> {
    if (data) {
      this.configs = {
        ...newConfigs(),
        ...data,
      };
    }
  }

  public async init(): Promise<void> {
    return new Promise<void>((resolve, reject) => {
      const init = async (data?: Configs) => {
        wsc.removeListener('configs', init);
        await this.onConfigs(data);
        wsc.addListener('configs', this.onConfigs);
        resolve();
      };
      wsc.addListener('configs', init);
      wsc.send('configsInit');
    });
  }

  public install($Vue: typeof Vue): void {
    if ($$Vue && $$Vue === $Vue) {
      return;
    }
    $$Vue = $Vue;
    const self = this;

    Object.defineProperty($Vue.prototype, '$configs', {
      get(): Configs {
        return self.configs;
      },
    });
  }
}
