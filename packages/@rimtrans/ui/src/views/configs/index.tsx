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

const sideItems: NavItem[] = [
  {
    icon: ['fas', 'globe'],
    label: 'configs_interface_languages',
    to: '/configs/interface-languages',
  },
  {
    icon: ['fas', 'cogs'],
    label: 'configs_application',
    to: '/configs/application',
  },
  {
    icon: ['fas', 'file-alt'],
    label: 'configs_core_languages',
    to: '/configs/core-languages',
  },
  {
    icon: ['fas', 'info-circle'],
    label: 'configs_about',
    to: '/configs/about',
  },
];

/**
 * Component: Configs
 */
@Component
export class VConfigs extends Vue implements ThemeComponent {
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
      <div staticClass="v-configs" class={this.classes}>
        <vd-swimlane>
          <vd-container>
            <vd-flexbox gap="xlarge">
              <vd-flexbox flex="none">
                <c-side-menu
                  items-source={sideItems.map<NavItem>(item => ({
                    ...item,
                    label: this.$locale.dict[item.label] || item.label,
                  }))}
                />
              </vd-flexbox>

              <vd-flexbox>
                <router-view staticClass="v-configs_wrapper va-init" />
              </vd-flexbox>
            </vd-flexbox>
          </vd-container>
        </vd-swimlane>
      </div>
    );
  }
}
