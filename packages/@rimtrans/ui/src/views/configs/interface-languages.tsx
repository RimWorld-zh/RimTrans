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
import { RadioData } from 'void-ui';

const auto: RadioData = {
  label: 'Auto',
  value: 'auto',
};
/**
 * Component: ConfigsInterfaceLanguages
 */
@Component
export class VConfigsInterfaceLanguages extends Vue {
  private render(h: CreateElement): VNode {
    return (
      <div staticClass="v-configs_interface-languages">
        <vd-radio-group
          direction="column"
          v-model={this.$configs.language}
          items-source={[auto].concat(languages)}
        />
      </div>
    );
  }
}
