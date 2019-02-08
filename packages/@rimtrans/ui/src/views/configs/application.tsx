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

const labelWidth = 35;

const pathRW = 'configs_application_path_rw';
const pathWS = 'configs_application_path_ws';

/**
 * Component: VConfigsApplication
 */
@Component
export class VConfigsApplication extends Vue {
  private render(h: CreateElement): VNode {
    return (
      <div staticClass="v-configs_application">
        <vd-flexbox direction="column" align="stretch" gap>
          <vd-flexbox align="center" gap>
            <vd-flexbox staticClass="v-configs_label" flex={labelWidth}>
              {this.$locale.dict[pathRW] || pathRW}
            </vd-flexbox>
            <vd-flexbox>
              <input
                staticClass="v-configs_text-input"
                type="text"
                v-model={this.$configs.pathToRimWorld}
                placeholder="e.g. .../Steam/steamapps/common/RimWorld"
              />
            </vd-flexbox>
          </vd-flexbox>

          <vd-flexbox align="center" gap>
            <vd-flexbox staticClass="v-configs_label" flex={labelWidth}>
              {this.$locale.dict[pathWS] || pathWS}
            </vd-flexbox>
            <vd-flexbox>
              <input
                staticClass="v-configs_text-input"
                type="text"
                v-model={this.$configs.pathToWorkshop}
                placeholder="e.g. .../Steam/steamapps/workshop/content/294100"
              />
            </vd-flexbox>
          </vd-flexbox>
        </vd-flexbox>
      </div>
    );
  }
}
