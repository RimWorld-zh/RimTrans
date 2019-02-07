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
import { NavItem } from '../models';

/**
 * Component: SideMenu
 */
@Component
export class CSideMenu extends Vue {
  @Prop({ type: Array, required: true })
  public readonly itemsSource!: NavItem[];

  private render(h: CreateElement): VNode {
    return (
      <div staticClass="c-side-menu">
        <vd-list divided gap>
          {this.itemsSource.map(item => (
            <vd-list-item
              staticClass="c-side-menu_item"
              active-class="is-active"
              exact
              router-link
              tag="a"
              to={item.to}
            >
              {item.icon && [<fa-icon staticClass="vda-icon_left" icon={item.icon} />]}
              {item.label}
            </vd-list-item>
          ))}
        </vd-list>
      </div>
    );
  }
}
