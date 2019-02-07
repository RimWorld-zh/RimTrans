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
import { NavItem } from '@src/components/models';

const brand: NavItem = {
  label: 'RimTrans',
  to: '/',
};
const headerItems: NavItem[] = [
  {
    icon: ['fas', 'cog'],
    label: 'configs',
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
          brand={brand}
          items-source={headerItems.map<NavItem>(item => ({
            ...item,
            label: this.$locale.dict[item.label] || item.label,
          }))}
        />
        <router-view staticClass="v-app_wrapper" />
      </div>
    );
  }
}
