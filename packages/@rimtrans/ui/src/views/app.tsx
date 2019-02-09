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
import { ClassName, Theme, ThemeComponent } from 'void-ui';
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
export class VApp extends Vue implements ThemeComponent {
  @Prop(String)
  public readonly theme?: Theme;
  public get themeValue(): Theme {
    return this.theme || this.$vd_theme.theme || 'lite';
  }

  public get classes(): ClassName {
    return [`vp-theme_${this.themeValue}`];
  }

  private render(h: CreateElement): VNode {
    return (
      <div staticClass="v-app" class={this.classes}>
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
