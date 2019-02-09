// tslint:disable:no-var-requires no-any no-unsafe-any
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
import packageJson from '../../../package.json';
import { version } from '@rimtrans/core';

/**
 * Component: ConfigsAbout
 */
@Component
export class VConfigsAbout extends Vue {
  private render(h: CreateElement): VNode {
    return (
      <div staticClass="v-configs_about">
        <vd-flexbox tag="article" direction="column" align="center" gap="small">
          <vd-flexbox tag="h1">RimTrans</vd-flexbox>
          <vd-flexbox tag="p">Developer: duduluu</vd-flexbox>
          <vd-flexbox tag="p">License: MIT</vd-flexbox>
          <vd-flexbox tag="p">Version: {packageJson.version}</vd-flexbox>
          <vd-flexbox tag="p">Support RimWorld Versions: {version}</vd-flexbox>
        </vd-flexbox>
      </div>
    );
  }
}
