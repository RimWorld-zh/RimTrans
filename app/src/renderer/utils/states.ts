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
import { IpcMessage } from '@src/main/utils/ipc';
import { StateTypeMap, StateChannel, Paths } from '@src/main/utils/states';
import { Settings } from '@src/main/utils/states/settings';
import { IpcRendererListener, IpcRenderer, createIpc, getGlobal } from './ipc';
import { StateI18n } from './i18n';
import { StateUi } from './ui';

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

  // Public
  /* eslint-disable lines-between-class-members,@typescript-eslint/no-explicit-any */
  public browserWindowID!: number;
  public ipc!: IpcRenderer;
  public paths!: Paths;
  public settings: Settings = null as any;

  private stateI18n!: StateI18n;
  public get i18n(): StateI18n['dict'] {
    return this.stateI18n.dict;
  }

  private stateUi!: StateUi;
  /* eslint-enable lines-between-class-members,@typescript-eslint/no-explicit-any */

  // Sync
  /* eslint-disable lines-between-class-members */
  private statesIpc!: IpcRenderer<StateTypeMap>;
  private statesIpcSilentMap!: Partial<Record<StateChannel, boolean>>;
  private unwatchMap!: Partial<Record<StateChannel, Function>>;
  private listenerMap!: Partial<Record<StateChannel, Function>>;
  /* eslint-enable lines-between-class-members */

  private installState<K extends StateChannel & keyof States>(
    channel: K,
    globalKey: string,
  ): void {
    this[channel] = getGlobal(globalKey);

    const watch = (data: StateTypeMap[K][0]): void => {
      if (this.statesIpcSilentMap[channel]) {
        this.statesIpcSilentMap[channel] = false;
        return;
      }
      this.statesIpc.send(channel, { id: this.browserWindowID, data });
    };

    const listener: IpcRendererListener<StateTypeMap[K][0]> = (event, message) => {
      if (message && message.id !== this.browserWindowID) {
        this.statesIpcSilentMap[channel] = true;
        this[channel] = message.data;
      }
    };

    const unwatch = this.$watch(channel, watch, { deep: true });
    this.unwatchMap[channel] = unwatch;

    this.listenerMap[channel] = listener;
    this.statesIpc.on(channel, listener);
  }

  private created(): void {
    this.browserWindowID = remote.getCurrentWindow().id;
    this.ipc = createIpc('app');
    this.paths = getGlobal(GLOBAL_KEY_PATHS);

    this.statesIpc = createIpc<StateTypeMap>('states');
    this.statesIpcSilentMap = {};
    this.unwatchMap = {};
    this.listenerMap = {};

    this.installState('settings', GLOBAL_KEY_SETTINGS);

    this.stateI18n = new StateI18n({ parent: this });
    this.stateUi = new StateUi({ parent: this });
  }

  private beforeDestroy(): void {
    (Object.values(this.unwatchMap) as Function[]).forEach(unwatch => unwatch());
    (Object.entries(this.listenerMap) as [
      StateChannel,
      IpcRendererListener<StateTypeMap[StateChannel][0]>
    ][]).forEach(([channel, listener]) =>
      this.statesIpc.removeListener(channel, listener),
    );
  }
}
