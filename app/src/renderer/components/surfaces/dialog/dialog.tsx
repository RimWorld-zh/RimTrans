/* eslint-disable @typescript-eslint/no-explicit-any */
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
import { ButtonColor, when } from '../../base';

let $$Vue: typeof Vue | undefined;

declare module 'vue/types/vue' {
  interface Vue {
    /**
     * Show a dialog under Promise API.
     * @param options the dialog options
     * @param parent the vue component instance which invoke this function
     * @param render the render function for default slot
     *
     */
    // eslint-disable-next-line @typescript-eslint/camelcase
    $rw_showDialog<T = undefined>(
      parent: Vue,
      options: DialogOptions,
      render?: (createElement: CreateElement, state: T) => VNode | VNode[] | undefined,
      initState?: () => T,
    ): Promise<[boolean, T]>;
  }
}

export interface DialogOptions {
  /**
   * Dialog title.
   */
  title?: string;
  /**
   * Dialog content, will be hidden when the default slot is exists.
   */
  content?: string;
  /**
   * If block dismiss event or not.
   * @default true
   */
  block?: boolean;
  /**
   * The label of the confirm button.
   */
  confirmLabel?: string;
  /**
   * The color of the confirm button.
   */
  cancelLabel?: string;
  /**
   * The label of the cancel button.
   */
  confirmColor?: ButtonColor;
  /**
   * The color of the cancel button.
   */
  cancelColor?: ButtonColor;
}

/**
 * Component: Dialog
 */
@Component
export class RwDialog extends Vue implements DialogOptions {
  public static install($Vue: typeof Vue): void {
    if ($$Vue && $$Vue === $Vue) {
      return;
    }
    $$Vue = $Vue;

    async function showDialog<T = undefined>(
      parent: Vue,
      options: DialogOptions,
      render?: (createElement: CreateElement, state: T) => VNode | VNode[] | undefined,
      initState: () => T = () => undefined as any,
    ): Promise<[boolean, T]> {
      return new Promise<[boolean, T]>((resolve, reject) => {
        const toggle = new $Vue({
          parent,
          data() {
            return {
              show: true,
              state: initState(),
            };
          },
        });
        const destroy = parent.$rw_portalRender('body', parent, h => {
          const content = render && render(h, toggle.state);

          const confirm = (e: MouseEvent | KeyboardEvent): void => {
            toggle.show = false;
            const state = toggle.state && JSON.parse(JSON.stringify(toggle.state));
            parent.$nextTick(() => {
              toggle.$destroy();
              destroy();
              resolve([true, state]);
            });
          };
          const cancel = (e: MouseEvent | KeyboardEvent): void => {
            toggle.show = false;
            const state = toggle.state && JSON.parse(JSON.stringify(toggle.state));
            parent.$nextTick(() => {
              toggle.$destroy();
              destroy();
              resolve([false, state]);
            });
          };

          return h(
            'rw-dialog',
            {
              props: { ...options, show: toggle.show },
              on: { confirm, cancel, close: cancel, dismiss: cancel },
            },
            (Array.isArray(content) && content) || [content],
          );
        });
      });
    }

    const typeCheck: Vue['$rw_showDialog'] = showDialog;

    Object.defineProperty(Vue.prototype, '$rw_showDialog', {
      configurable: false,
      enumerable: false,
      writable: false,
      value: showDialog,
    });
  }

  @Prop({ type: Boolean, required: true })
  public readonly show!: boolean;

  @Prop(String)
  public readonly title?: string;

  @Prop(String)
  public readonly content?: string;

  @Prop({ type: Boolean, default: true })
  public readonly block!: boolean;

  @Prop(String)
  public readonly confirmLabel?: string;

  @Prop(String)
  public readonly cancelLabel?: string;

  @Prop({ type: String, default: 'primary' })
  public readonly confirmColor!: ButtonColor;

  @Prop({ type: String, default: 'default' })
  public readonly cancelColor?: ButtonColor;

  @Prop({ type: Boolean, default: true })
  public readonly acrylic!: boolean;

  private destroy?: Function;

  @Watch('show')
  private watchShow(show: boolean): void {
    if (show && !this.destroy) {
      this.$nextTick(() => {
        this.destroy = this.$rw_portalRender('body', this, h => this.renderPortal(h));
      });
    }

    if (!show) {
      this.$nextTick(() => {
        if (this.destroy) {
          this.destroy();
          this.destroy = undefined;
        }
      });
    }
  }

  private mounted(): void {
    this.watchShow(this.show);
  }

  private beforeDestroy(): void {
    this.watchShow(false);
  }

  private renderPortal(h: CreateElement): VNode {
    const { show } = this;
    if (!show) {
      return h();
    }

    const {
      title,
      content,
      block,
      confirmLabel,
      cancelLabel,
      confirmColor,
      cancelColor,
      acrylic,
      $slots: { header: headerSlot, default: defaultSlot, footer: footerSlot },
      $listeners: {
        confirm = [],
        cancel = [],
        close = [],
        dismiss = [],
        keydown: rawKeydown = [],
        ...on
      },
    } = this;

    const emitRaw = (listener: Function | Function[], event: Event): void => {
      if (Array.isArray(listener)) {
        listener.forEach(l => l(event));
        return;
      }
      listener(event);
    };

    const keydown = (event: KeyboardEvent): void => {
      switch (event.key) {
        case 'Enter':
          emitRaw(confirm, event);
          break;

        case 'Escape':
          emitRaw(cancel, event);
          break;

        default:
      }
      emitRaw(rawKeydown, event);
    };

    const overlayClasses = when({ acrylic });

    return (
      <div
        staticClass="rw-dialog"
        {...{
          on: {
            ...on,
            keydown,
          },
        }}
      >
        <div
          key="overlay"
          staticClass="rw-dialog_overlay"
          class={overlayClasses}
          onClick={(!block && dismiss) || []}
        />
        <div key="container" staticClass="rw-dialog_container">
          <header key="header" staticClass="rw-dialog_header">
            <span key="header-content" staticClass="rw-dialog_header-content">
              {headerSlot || title}
            </span>
            <rw-button
              key="close-button"
              staticClass="rw-dialog_close-button"
              skin="flat"
              shape="square"
              onClick={close}
            >
              <md-icon staticClass="rw-button-icon" icon="Close" />
            </rw-button>
          </header>

          <div key="body" staticClass="rw-dialog_body">
            {defaultSlot || content}
          </div>

          {!!(footerSlot || confirmLabel || cancelLabel) && (
            <footer key="footer" staticClass="rw-dialog_footer">
              {footerSlot}
              {!!confirmLabel && (
                <rw-button key="confirm-button" color={confirmColor} onClick={confirm}>
                  {confirmLabel}
                </rw-button>
              )}
              {!!cancelLabel && (
                <rw-button key="cancel-button" color={cancelColor} onClick={cancel}>
                  {cancelLabel}
                </rw-button>
              )}
            </footer>
          )}
        </div>
      </div>
    );
  }

  private render(h: CreateElement): undefined {
    return undefined;
  }
}
