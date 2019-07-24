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
];

/**
 * Component: DevTools
 */
@Component
export class RwDevTools extends Vue {
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

  private render(h: CreateElement): VNode {
    return (
      <div staticClass="rw-dev-tools">
        <rw-button key="label" size="small" skin="flat" shape="square">
          <mdi staticClass="rw-button-icon" icon="DevTo" />
        </rw-button>

        <rw-button
          key="reload"
          size="small"
          skin="flat"
          shape="square"
          title="Toggle DevTools"
          onClick={this.onReload}
        >
          <mdi staticClass="rw-button-icon" icon="Reload" />
        </rw-button>

        <rw-button
          key="dev-tools"
          size="small"
          skin="flat"
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
            skin="flat"
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
