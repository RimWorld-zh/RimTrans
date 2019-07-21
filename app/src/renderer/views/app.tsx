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

/**
 * Component: App
 */
@Component
export default class VApp extends Vue {
  private isDevelopment: boolean = process.env.NODE_ENV === 'development';

  private render(h: CreateElement): VNode {
    return (
      <div staticClass="v-app">
        <div staticClass="v-app_bar">
          <rw-button size="large" skin="flat" shape="square" router-link to="/">
            <mdi staticClass="v-app_bar-icon" icon="Home" />
          </rw-button>

          <rw-button
            size="large"
            skin="flat"
            shape="square"
            onClick={() => window.history.back()}
          >
            <mdi staticClass="v-app_bar-icon" icon="ChevronLeft" />
          </rw-button>
          <rw-button
            size="large"
            skin="flat"
            shape="square"
            onClick={() => window.history.forward()}
          >
            <mdi staticClass="v-app_bar-icon" icon="ChevronRight" />
          </rw-button>

          <i staticClass="v-app_bar-blank" />

          <rw-button size="large" skin="flat" shape="square" router-link to="/settings">
            <mdi staticClass="v-app_bar-icon" icon="Settings" />
          </rw-button>
        </div>

        <div staticClass="v-app_wrapper">
          <router-view staticClass="rw-container" />
        </div>

        {this.isDevelopment && (
          <div staticClass="v-app_dev-tools">
            <button
              onClick={() => {
                const win = remote.getCurrentWindow();
                win.webContents.openDevTools();
              }}
            >
              DevTools
            </button>
            <router-link staticClass="v-app_dev-tools-item" to="/dev-tools/icons">
              icons
            </router-link>
            <span>{decodeURIComponent(this.$route.fullPath)}</span>
          </div>
        )}
      </div>
    );
  }
}
