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
 * Component: Header
 */
@Component
export class CHeader extends Vue {
  @Prop({ type: Object, required: true })
  public readonly brand!: NavItem;

  @Prop({ type: Array, required: true })
  public readonly itemsSource!: NavItem[];

  private render(h: CreateElement): VNode {
    return (
      <div staticClass="c-header">
        <vd-container staticClass="c-header_container">
          <vd-flexbox
            staticClass="c-header_wrapper"
            align-self="stretch"
            align="stretch"
            gap
          >
            <vd-flexbox
              staticClass="c-header_item"
              flex="none"
              justify="center"
              align="center"
              router-link
              tag="a"
              to={this.brand.to}
            >
              {this.brand.label}
            </vd-flexbox>
            <vd-flexbox />
            <vd-flexbox />
            {this.itemsSource.map(item => (
              <vd-flexbox
                staticClass="c-header_item"
                flex="none"
                justify="center"
                align="center"
                router-link
                tag="a"
                to={item.to}
                title={item.label}
              >
                {item.icon ? <fa-icon icon={item.icon} /> : item.label}
              </vd-flexbox>
            ))}
          </vd-flexbox>
        </vd-container>
      </div>
    );
  }
}
