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
import { HeaderItem } from '@src/components/all';

const headerItems: HeaderItem[] = [
  {
    label: 'Configs',
    to: '/configs',
  },
];

/**
 * Component: App
 */
@Component
export class VApp extends Vue {
  private render(h: CreateElement): VNode {
    return (
      <div staticClass="v-app">
        <c-header
          brand={{
            label: 'RimTrans',
            to: '/',
          }}
          items-source={headerItems}
        />
        <router-view staticClass="v-app_wrapper" />
      </div>
    );
  }
}
