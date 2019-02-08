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
import { languages } from '@src/i18n';

/**
 * Component: InterfaceLanguages
 */
@Component
export class VInterfaceLanguages extends Vue {
  private render(h: CreateElement): VNode {
    return (
      <div staticClass="v-interface-languages">
        <vd-radio-group
          direction="column"
          v-model={this.$configs.language}
          items-source={languages}
        />
      </div>
    );
  }
}
