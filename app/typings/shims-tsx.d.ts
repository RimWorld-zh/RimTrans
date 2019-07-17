/* eslint-disable @typescript-eslint/no-unused-vars,@typescript-eslint/no-empty-interface,@typescript-eslint/no-explicit-any */
import Vue, { VNode, RenderContext } from 'vue';
import { DefaultProps } from 'vue/types/options';

declare global {
  namespace JSX {
    interface Element extends VNode {}
    interface ElementClass extends Vue {}
    interface IntrinsicElements {
      [elem: string]: any;
    }
  }
}
