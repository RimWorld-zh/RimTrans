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
    const {
      $i18n: {
        dict: {
          settings: { features, ui, about },
        },
      },
    } = this;

    const categories: SettingCategory[] = [
      {
        icon: 'Translate',
        label: 'Language', // DO NOT use translation for this category
        to: 'language',
      },
      {
        icon: 'FunctionVariant',
        label: features,
        to: 'features',
      },
      {
        icon: 'Palette',
        label: ui,
        to: 'ui',
      },
    ];

    const { path } = this.$route;
    const stripOffset = categories.findIndex(({ to }) => path.includes(to));

    return (
      <div staticClass="v-settings">
        <div key="sidebar" staticClass="v-settings_sidebar">
          {categories.map(({ icon, label, to }, index) => (
            <rw-button
              key={to}
              staticClass="v-settings_category"
              size="large"
              skin={(stripOffset === index && 'fill') || 'flat'}
              router-link
              to={`/settings/${to}`}
              exact-active-class="is-active"
            >
              <md-icon staticClass="rw-button-icon rw-icon-left" icon={icon} />
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
          <div key={stripOffset} staticClass="v-settings_panel">
            <h1 staticClass="v-settings_title">{categories[stripOffset].label}</h1>
            <router-view />
          </div>
        </transition>
      </div>
    );
  }
}
