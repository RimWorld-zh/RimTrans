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
import * as MdiIconsMap from '@mdi/js';

/**
 * Component: Icons
 */
@Component
export default class VDevToolsIcons extends Vue {
  private render(h: CreateElement): VNode {
    return (
      <div staticClass="v-dev-tools-icons">
        <h1>Material Design Icons</h1>
        <ul staticClass="v-dev-tools-icons_wrapper">
          {Object.keys(MdiIconsMap)
            .sort()
            .map(key => {
              const icon = key.replace('mdi', '');
              return (
                <li key={icon} staticClass="v-dev-tools-icons_item">
                  <button staticClass="v-dev-tools-icons_container" title={icon}>
                    <md-icon staticClass="v-dev-tools-icons_icon" icon={icon} />
                  </button>
                  <span staticClass="v-dev-tools-icons_label">{icon}</span>
                </li>
              );
            })}
        </ul>

        <h2>Size</h2>
        <ul staticClass="v-dev-tools-icons_wrapper">
          <li staticClass="v-dev-tools-icons_item">
            <button staticClass="v-dev-tools-icons_container" title={18}>
              <md-icon staticClass="v-dev-tools-icons_icon" icon="Account" size={18} />
            </button>
          </li>
          <li staticClass="v-dev-tools-icons_item">
            <button staticClass="v-dev-tools-icons_container" title={24}>
              <md-icon staticClass="v-dev-tools-icons_icon" icon="Account" size={24} />
            </button>
          </li>
          <li staticClass="v-dev-tools-icons_item">
            <button staticClass="v-dev-tools-icons_container" title={36}>
              <md-icon staticClass="v-dev-tools-icons_icon" icon="Account" size={36} />
            </button>
          </li>
          <li staticClass="v-dev-tools-icons_item">
            <button staticClass="v-dev-tools-icons_container" title={48}>
              <md-icon staticClass="v-dev-tools-icons_icon" icon="Account" size={48} />
            </button>
          </li>
        </ul>

        <h2>Rotate</h2>
        <ul staticClass="v-dev-tools-icons_wrapper">
          {Array(8)
            .fill(0)
            .map((_, multiple) => {
              const rotate = 45 * multiple;
              return (
                <li key={rotate} staticClass="v-dev-tools-icons_item">
                  <button staticClass="v-dev-tools-icons_container" title={rotate}>
                    <md-icon
                      staticClass="v-dev-tools-icons_icon"
                      icon="Account"
                      rotate={rotate}
                    />
                  </button>
                </li>
              );
            })}
        </ul>

        <h2>Flip</h2>
        <ul staticClass="v-dev-tools-icons_wrapper">
          <li staticClass="v-dev-tools-icons_item">
            <button staticClass="v-dev-tools-icons_container" title="flip-h">
              <md-icon staticClass="v-dev-tools-icons_icon" icon="AccountAlert" flip-h />
            </button>
          </li>
          <li staticClass="v-dev-tools-icons_item">
            <button staticClass="v-dev-tools-icons_container" title="flip-v">
              <md-icon staticClass="v-dev-tools-icons_icon" icon="AccountAlert" flip-v />
            </button>
          </li>
          <li staticClass="v-dev-tools-icons_item">
            <button staticClass="v-dev-tools-icons_container" title="flip-h flip-v">
              <md-icon
                staticClass="v-dev-tools-icons_icon"
                icon="AccountAlert"
                flip-h
                flip-v
              />
            </button>
          </li>
        </ul>

        <h2>Spin</h2>
        <ul staticClass="v-dev-tools-icons_wrapper">
          <li staticClass="v-dev-tools-icons_item">
            <button staticClass="v-dev-tools-icons_container" title="Loading spin">
              <md-icon staticClass="v-dev-tools-icons_icon" icon="Loading" spin />
            </button>
          </li>
          <li staticClass="v-dev-tools-icons_item">
            <button staticClass="v-dev-tools-icons_container" title="Star spin">
              <md-icon staticClass="v-dev-tools-icons_icon" icon="Star" spin />
            </button>
          </li>
        </ul>

        <h2>Color</h2>
        <ul staticClass="v-dev-tools-icons_wrapper">
          {['blue', 'teal', 'magenta', 'green', 'yellow', 'red', 'purple', 'orange'].map(
            color => (
              <li staticClass="v-dev-tools-icons_item">
                <button staticClass="v-dev-tools-icons_container" title={color}>
                  <md-icon
                    staticClass="v-dev-tools-icons_icon"
                    icon="Star"
                    style={{ color: `var(--color-${color}-50)` }}
                  />
                </button>
              </li>
            ),
          )}
        </ul>
      </div>
    );
  }
}
