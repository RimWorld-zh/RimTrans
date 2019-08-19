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
 * Component: DevToolsPlayground
 */
@Component
export default class VDevToolsPlayground extends Vue {
  private percent: number = 0;

  private handler!: number;

  private mounted(): void {
    this.handler = window.setInterval(() => {
      if (this.percent >= 100) {
        this.percent = 0;
        return;
      }
      this.percent += 1;
    }, 100);
  }

  private beforeDestroy(): void {
    window.clearInterval(this.handler);
  }

  private render(h: CreateElement): VNode {
    const { percent } = this;

    return (
      <div staticClass="v-dev-tools-playground">
        <div staticClass="v-dev-tools-playground_row">
          <rw-progress-indicator percent-complete={percent / 100} />
        </div>
        <div staticClass="v-dev-tools-playground_row">
          <rw-progress-indicator />
        </div>
      </div>
    );
  }
}
