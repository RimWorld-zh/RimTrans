import Vue, { CreateElement, VNode, PluginObject } from 'vue';
import {
  Component,
  Emit,
  Inject,
  Model,
  Prop,
  Provide,
  Watch,
} from 'vue-property-decorator';

let $$Vue: typeof Vue | undefined;

declare module 'vue/types/vue' {
  interface Vue {
    /**
     * Render content anywhere you want, returns a destroy handler function.
     * @param container the container element or selector, where render inside to
     * @param parent the vue component instance which invoke this function
     * @param render the render function for default slot
     */
    // eslint-disable-next-line @typescript-eslint/camelcase
    $rw_portalRender(
      container: Element | string,
      parent: Vue,
      render: (createElement: CreateElement) => VNode,
    ): Function;
  }
}

/**
 * Plugin Portal Render
 */
export const RwPluginPortalRender: PluginObject<undefined> = {
  install($Vue: typeof Vue): void {
    if ($$Vue && $$Vue === $Vue) {
      return;
    }
    $$Vue = $Vue;

    const portalRender: Vue['$rw_portalRender'] = (container, parent, render) => {
      if (parent.$isServer) {
        return () => {};
      }

      const element = window.document.createElement('div');
      const target =
        typeof container === 'string' ? document.querySelector(container) : container;

      if (!target) {
        throw new Error(`Element or selector not found: ${container}`);
      }

      target.appendChild(element);

      const portal = new $Vue({
        parent,
        render,
      });

      portal.$mount(element);

      return () => {
        target.removeChild(portal.$el);
        portal.$destroy();
      };
    };

    Object.defineProperty($Vue.prototype, '$rw_portalRender', {
      configurable: false,
      enumerable: false,
      writable: false,
      value: portalRender,
    });
  },
};
