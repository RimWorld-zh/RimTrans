import { remote } from 'electron';
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

interface DebugItem {
  icon: string;
  label: string;
  to: string;
}

const items: DebugItem[] = [
  {
    icon: 'VectorSquare',
    label: 'Material Design Icons',
    to: '/dev-tools/icons',
  },
  {
    icon: 'Palette',
    label: 'Colors',
    to: '/dev-tools/colors',
  },
];

/**
 * Component: DevTools
 */
@Component
export class RwDevTools extends Vue {
  private showSettings: boolean = false;

  private onToggleShowSettings(event: Event): void {
    this.showSettings = !this.showSettings;
  }

  private onReload(event: MouseEvent): void {
    remote.getCurrentWindow().reload();
  }

  private onToggleDevTools(event: MouseEvent): void {
    const web = remote.getCurrentWebContents();
    if (web.isDevToolsOpened()) {
      web.closeDevTools();
    } else {
      web.openDevTools();
    }
  }

  private serializeSettings(): string {
    return JSON.stringify(this.$states.settings, undefined, '  ');
  }

  private serializeProcess(): string {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const data: Record<string, any> = {};
    const properties: (keyof NodeJS.Process)[] = [
      'argv',
      'argv0',
      'execPath',
      'execArgv',
      'env',
      'version',
      'versions',
    ];
    properties.forEach(p => {
      data[p] = remote.process[p];
    });
    return JSON.stringify(data, undefined, '  ');
  }

  private render(h: CreateElement): VNode {
    const { showSettings } = this;

    return (
      <div staticClass="rw-dev-tools">
        {showSettings && (
          <div key="settings" staticClass="rw-dev-tools_settings">
            <pre staticClass="rw-dev-tools_settings-content">
              <code>{this.serializeSettings()}</code>
              <br />
              <br />
              <code>{this.serializeProcess()}</code>
            </pre>
          </div>
        )}

        <rw-button
          key="label"
          size="small"
          color="primary"
          skin="fill"
          shape="square"
          title="Toggle show settings"
          onClick={this.onToggleShowSettings}
        >
          <mdi staticClass="rw-button-icon" icon="DevTo" />
        </rw-button>

        <rw-button
          key="reload"
          size="small"
          color="primary"
          skin="fill"
          shape="square"
          title="Reload"
          onClick={this.onReload}
        >
          <mdi staticClass="rw-button-icon" icon="Reload" />
        </rw-button>

        <rw-button
          key="dev-tools"
          size="small"
          color="primary"
          skin="fill"
          shape="square"
          title="Toggle DevTools"
          onClick={this.onToggleDevTools}
        >
          <mdi staticClass="rw-button-icon" icon="Bug" />
        </rw-button>

        {items.map(item => (
          <rw-button
            key={item.to}
            title={item.label}
            router-link
            to={item.to}
            size="small"
            color="primary"
            skin="fill"
            shape="square"
          >
            <mdi staticClass="rw-button-icon" icon={item.icon} />
          </rw-button>
        ))}

        <span staticClass="rw-dev-tools_route">{this.$route.name}</span>
        <span staticClass="rw-dev-tools_route">{this.$route.path}</span>
      </div>
    );
  }
}
