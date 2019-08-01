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
import { when, GOLDEN_RATIO } from '@src/renderer/components/base';

interface NavItem {
  icon: string;
  color: string;
  label: string;
  to: string;
  disabled?: boolean;
}

const navList: NavItem[] = [
  {
    icon: 'Translate',
    color: 'green',
    label: 'Translator',
    to: '/translator',
  },
  {
    icon: 'Codepen',
    color: 'blue',
    label: 'Modder',
    to: '/modder',
  },
];

/**
 * Component: Welcome
 */
@Component
export default class VWelcome extends Vue {
  private render(h: CreateElement): VNode {
    return (
      <div staticClass="v-welcome rw-container">
        <div staticClass="v-welcome_wrapper">
          <div key="hero" staticClass="v-welcome_item is-hero">
            <div staticClass="v-welcome_hero">
              <rw-aspect-ratio ratio={2 / GOLDEN_RATIO}>
                <rw-brand>RimTrans</rw-brand>
              </rw-aspect-ratio>
            </div>
          </div>

          {navList.map(({ icon, color, label, to, disabled }) => (
            <div key={to} staticClass="v-welcome_item">
              <router-link
                staticClass="v-welcome_nav"
                class={when({ disabled })}
                disabled={disabled}
                to={to}
              >
                <rw-aspect-ratio golden>
                  <div
                    staticClass="v-welcome_nav-icon-container"
                    style={{ color: `var(--color-${color}-50)` }}
                  >
                    <mdi
                      staticClass="v-welcome_nav-icon"
                      icon={icon}
                      style={{ color: `var(--color-${color}-90)` }}
                    />
                  </div>
                </rw-aspect-ratio>
                <span staticClass="v-welcome_nav-label">{label}</span>
              </router-link>
            </div>
          ))}
        </div>
      </div>
    );
  }
}
