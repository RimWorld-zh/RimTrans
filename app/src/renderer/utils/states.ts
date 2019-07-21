import { remote, ipcRenderer, Event as ElectronEvent } from 'electron';
import Vue, { CreateElement, VNode, PluginFunction, PluginObject } from 'vue';
import {
  Component,
  Emit,
  Inject,
  Model,
  Prop,
  Provide,
  Watch,
} from 'vue-property-decorator';
import { GLOBAL_KEY_PATHS, GLOBAL_KEY_SETTINGS } from '@src/main/utils/constants';
import { IpcChannel, IpcMessage, IpcListener, Paths } from '@src/main/utils/states';
import { Settings } from '@src/main/utils/states/settings';

// ----------------------------------------------------------------
// IPC

/* eslint-disable @typescript-eslint/no-explicit-any */

export function ipcOn<T extends IpcChannel>(channel: T, listener: IpcListener<T>): void {
  ipcRenderer.on(channel, listener);
}

export function ipcOnce<T extends IpcChannel>(
  channel: T,
  listener: IpcListener<T>,
): void {
  ipcRenderer.once(channel, listener);
}

export function ipcSend<T extends IpcChannel>(channel: T, data: IpcMessage<T>): void {
  ipcRenderer.send(channel, JSON.parse(JSON.stringify(data)));
}

export function ipcRemoveListener<T extends IpcChannel>(
  channel: T,
  listener: IpcListener<T>,
): void {
  ipcRenderer.removeListener(channel, listener);
}

/* eslint-enable @typescript-eslint/no-explicit-any */

// ----------------------------------------------------------------
// States

declare module 'vue/types/vue' {
  interface Vue {
    readonly $states: States;
  }
}

declare module 'vue/types/options' {
  interface ComponentOptions<V extends Vue> {
    states?: States;
  }
}

let $$Vue: typeof Vue | undefined;

/**
 * Plugin States, for sync shared data between electron main and renderer progresses.
 */
@Component
export class States extends Vue {
  public static install($Vue: typeof Vue): void {
    if ($$Vue && $$Vue === $Vue) {
      return;
    }
    $$Vue = $Vue;

    Object.defineProperty(Vue.prototype, '$states', {
      configurable: false,
      enumerable: false,
      get(this: Vue): States {
        return this.$root.$options.states as States;
      },
    });
  }

  public browserWindowID: number = remote.getCurrentWindow().id;

  public paths: Paths = remote.getGlobal(GLOBAL_KEY_PATHS);

  // --------------------------------
  // Settings

  public settings: Settings = remote.getGlobal(GLOBAL_KEY_SETTINGS);

  @Watch('settings', { deep: true })
  private watchSettings(value: Settings): void {
    ipcSend('settings', { id: this.browserWindowID, data: value });
  }

  private onIpcSettings(event: ElectronEvent, message: IpcMessage<'settings'>): void {
    if (message.id !== this.browserWindowID) {
      this.settings = message.data;
    }
  }

  private listeners!: Function[];

  private created(): void {
    this.listeners = [];
  }

  private beforeDestroy(): void {}
}
