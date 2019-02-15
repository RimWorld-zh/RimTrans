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
import { mods } from '@rimtrans/service';
import worker from '@rimtrans/worker';

/**
 * Component: ExtractorProjects
 */
@Component
export class VExtractorProjects extends Vue {
  private async mounted(): Promise<void> {
    const [internal, workshop] = await Promise.all([
      mods.get({
        params: {
          path: `${this.$configs.pathToRimWorld}/Mods`,
        },
      }),
      mods.get({
        params: {
          path: this.$configs.pathToWorkshop,
        },
      }),
    ]);

    [...internal, ...workshop].forEach(([path, xml]) => {
      try {
        const data = worker.modMetaData(xml);
        console.log(path, data.name);
      } catch (error) {
        console.error(error);
      }
    });
  }

  private render(h: CreateElement): VNode {
    return (
      <div staticClass="v-extractor_projects">
        <vd-tabs>
          <vd-tab-pane>
            <span slot="label">
              <fa-icon staticClass="vda-icon_left" icon={['fas', 'file-export']} />
              internal
            </span>
            <vd-flexbox />
          </vd-tab-pane>

          <vd-tab-pane>
            <span slot="label">
              <fa-icon icon={['fas', 'puzzle-piece']} />
              internal
            </span>
            <vd-flexbox />
          </vd-tab-pane>

          <vd-tab-pane>
            <span slot="label">
              <fa-icon icon={['fab', 'steam']} />
              internal
            </span>
            <vd-flexbox />
          </vd-tab-pane>

          <vd-tab-pane>
            <span slot="label">
              <fa-icon icon={['fas', 'folder']} />
              internal
            </span>
            <vd-flexbox />
          </vd-tab-pane>
        </vd-tabs>
      </div>
    );
  }
}
