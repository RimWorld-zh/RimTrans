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
          <router-link tag="button" to="/">
            <mdi icon="Home" />
          </router-link>
          <button onClick={() => window.history.back()}>
            <mdi icon="ChevronLeft" />
          </button>
          <button onClick={() => window.history.forward()}>
            <mdi icon="ChevronRight" />
          </button>
          <i staticClass="v-app_bar-blank" />
          <router-link tag="button" to="/settings">
            <mdi icon="Settings" />
          </router-link>
        </div>

        <div staticClass="v-app_wrapper">
          <router-view staticClass="c-container" />
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
