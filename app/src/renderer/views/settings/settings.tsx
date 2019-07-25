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
import { MOTION_DURATION_2 } from '@src/renderer/components/base';

interface SettingCategory {
  icon: string;
  label: string;
  to: string;
}

/**
 * Component: Settings
 */
@Component
export default class VSettings extends Vue {
  private render(h: CreateElement): VNode {
    const { dict } = this.$i18n;
    const categories: SettingCategory[] = [
      {
        icon: 'Translate',
        label: 'Language', // DO NOT use translation for this label
        to: 'language',
      },
      {
        icon: 'Settings',
        label: dict.settingsCategoryApplication,
        to: 'application',
      },
    ];

    const { path } = this.$route;
    const stripOffset = categories.findIndex(({ to }) => path.includes(to));

    return (
      <div staticClass="v-settings">
        <div key="sidebar" staticClass="v-settings_sidebar">
          {categories.map(({ icon, label, to }) => (
            <rw-button
              key={to}
              staticClass="v-settings_category"
              size="large"
              skin="flat"
              router-link
              to={`/settings/${to}`}
              exact-active-class="is-active"
            >
              <mdi staticClass="rw-button-icon rw-icon-left" icon={icon} />
              {label}
            </rw-button>
          ))}

          <rw-strip-v
            key="strip"
            staticClass="v-settings_sidebar-strip"
            size="large"
            offset={stripOffset}
          />
        </div>

        <transition name="rw-m-rise-fall" mode="out-in" duration={MOTION_DURATION_2}>
          <router-view staticClass="v-settings_form" />
        </transition>
      </div>
    );
  }
}
