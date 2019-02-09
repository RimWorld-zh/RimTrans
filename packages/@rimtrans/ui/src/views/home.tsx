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
import { NavItem } from '@src/components/all';

const navItems: NavItem[] = [
  {
    icon: ['fas', 'file-export'],
    label: 'extractor',
    to: '/extractor',
  },
  {
    icon: ['fas', 'cog'],
    label: 'configs',
    to: '/configs',
  },
];

/**
 * Component: Home
 */
@Component
export class VHome extends Vue implements ThemeComponent {
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
      <div staticClass="v-home" class={this.classes}>
        <vd-swimlane>
          <vd-container>
            <vd-flexbox gap>
              {navItems.map(item => (
                <vd-flexbox flex={25}>
                  <vd-card
                    staticClass="v-home_nav-card"
                    router-link
                    tag="a"
                    to={item.to}
                    bordered
                    hover-raise={1}
                    center
                  >
                    <vd-uniform-scale ratio={9 / 16}>
                      <div staticClass="v-home_card-icon">
                        <fa-icon icon={item.icon} />
                      </div>
                    </vd-uniform-scale>
                    <vd-card-content>
                      <vd-card-title>
                        {this.$locale.dict[item.label] || item.label}
                      </vd-card-title>
                    </vd-card-content>
                  </vd-card>
                </vd-flexbox>
              ))}
            </vd-flexbox>
          </vd-container>
        </vd-swimlane>
      </div>
    );
  }
}
