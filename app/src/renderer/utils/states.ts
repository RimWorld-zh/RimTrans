/* eslint-disable @typescript-eslint/no-explicit-any */
import { remote, Event as ElectronEvent, ipcRenderer } from 'electron';
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
import { IpcMessage, IpcListener } from '@src/main/utils/ipc';
import { StateTypeMap, StateChannel, Paths } from '@src/main/utils/states';
import { Settings } from '@src/main/utils/states/settings';
import { IpcRenderer, createIpc, getGlobal } from './ipc';

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

  public browserWindowID!: number;

  public paths!: Paths;

  public settings: Settings = getGlobal(GLOBAL_KEY_SETTINGS);

  // Sync

  private ipc!: IpcRenderer<StateTypeMap>;

  private ipcSilentMap!: Partial<Record<StateChannel, boolean>>;

  private unwatchMap!: Partial<Record<StateChannel, Function>>;

  private listenerMap!: Partial<Record<StateChannel, Function>>;

  private installState<K extends StateChannel>(channel: K): void {
    const watch = (data: StateTypeMap[K]): void => {
      if (this.ipcSilentMap[channel]) {
        this.ipcSilentMap[channel] = false;
        return;
      }
      this.ipc.send(channel, { id: this.browserWindowID, data });
    };
    const unwatch = this.$watch(channel, watch, { deep: true });
    this.unwatchMap[channel] = unwatch;

    const listener: IpcListener<StateTypeMap[K]> = (event, message) => {
      if (message && message.id !== this.browserWindowID) {
        this.ipcSilentMap[channel] = true;
        (this as any)[channel] = message.data;
      }
    };
    this.ipc.on(channel, listener);
  }

  private created(): void {
    this.browserWindowID = remote.getCurrentWindow().id;
    this.paths = remote.getGlobal(GLOBAL_KEY_PATHS);

    this.ipc = createIpc<StateTypeMap>();
    this.ipcSilentMap = {};
    this.unwatchMap = {};
    this.listenerMap = {};

    this.installState('settings');
  }

  private beforeDestroy(): void {
    (Object.values(this.unwatchMap) as Function[]).forEach(unwatch => unwatch());
    (Object.entries(this.listenerMap) as [
      StateChannel,
      IpcListener<StateTypeMap[StateChannel]>
    ][]).forEach(([channel, listener]) => this.ipc.removeListener(channel, listener));
  }
}
