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
import { GOLDEN_RATIO } from '@src/renderer/components/base';

interface NavItem {
  icon: string;
  color: string;
  label: string;
  to: string;
}

const navList: NavItem[] = [
  {
    icon: 'Translate',
    color: 'teal',
    label: 'Translator',
    to: '/translator',
  },
  {
    icon: 'Codepen',
    color: 'green',
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
      <div staticClass="v-welcome">
        <div staticClass="v-welcome_wrapper">
          <div key="hero" staticClass="v-welcome_item is-hero">
            <div staticClass="v-welcome_hero">
              <rw-aspect-ratio ratio={2 / GOLDEN_RATIO}>
                <rw-brand>RimTrans</rw-brand>
              </rw-aspect-ratio>
            </div>
          </div>

          {navList.map(nav => (
            <div key={nav.to} staticClass="v-welcome_item">
              <router-link staticClass="v-welcome_nav" to={nav.to}>
                <rw-aspect-ratio golden>
                  <div
                    staticClass="v-welcome_nav-icon-container"
                    style={{ color: `var(--color-${nav.color}-50)` }}
                  >
                    <mdi
                      staticClass="v-welcome_nav-icon"
                      icon={nav.icon}
                      style={{ color: `var(--color-${nav.color}-90)` }}
                    />
                  </div>
                </rw-aspect-ratio>
                <span staticClass="v-welcome_nav-label">{nav.label}</span>
              </router-link>
            </div>
          ))}
        </div>
      </div>
    );
  }
}
