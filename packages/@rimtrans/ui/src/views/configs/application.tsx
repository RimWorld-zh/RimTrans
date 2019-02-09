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
import { Configs } from '@src/utils/plugin-configs';

const labelWidth = 35;

const pathRW = 'configs_application_path_rw';
const pathWS = 'configs_application_path_ws';
const pathCU = 'configs_application_path_cu';

interface ConfigsOption {
  icon: [string, string];
  iconColor?: string;
  label: string;
  key: keyof Configs;
  placeholder?: string;
}

/**
 * Component: VConfigsApplication
 */
@Component
export class VConfigsApplication extends Vue {
  private static optionsText: ConfigsOption[] = [
    {
      icon: ['fas', 'star'],
      iconColor: 'rimworld',
      label: 'configs_application_path_rimworld',
      key: 'pathToRimWorld',
      placeholder: 'e.g. .../Steam/steamapps/common/RimWorld',
    },
    {
      icon: ['fab', 'steam'],
      iconColor: 'steam',
      label: 'configs_application_path_workshop',
      key: 'pathToWorkshop',
      placeholder: 'e.g. .../Steam/steamapps/workshop/content/294100',
    },
    {
      icon: ['fas', 'folder'],
      iconColor: 'folder',
      label: 'configs_application_path_custom',
      key: 'pathToCustom',
    },
  ];

  private render(h: CreateElement): VNode {
    return (
      <div staticClass="v-configs_application">
        <vd-flexbox gap>
          {VConfigsApplication.optionsText.map(opt => (
            <vd-flexbox flex={100} align="center" gap>
              <vd-flexbox staticClass="v-configs_label" flex={labelWidth} align="center">
                <fa-icon
                  staticClass="vda-icon_left"
                  class={{ [`ca-color_${opt.iconColor}`]: !!opt.iconColor }}
                  icon={opt.icon}
                  size="lg"
                />
                {this.$locale.dict[opt.label] || opt.label}
              </vd-flexbox>
              <vd-flexbox>
                <input
                  staticClass="v-configs_text-input"
                  type="text"
                  v-model={this.$configs[opt.key]}
                  placeholder={opt.placeholder}
                />
              </vd-flexbox>
            </vd-flexbox>
          ))}
        </vd-flexbox>
      </div>
    );
  }
}
