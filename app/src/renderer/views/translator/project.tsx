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
import { EXT_NAME_TRANSLATOR_PROJECT } from '@src/main/utils/constants';
import { cloneObject } from '@src/main/utils/object';
import { interaction } from '@src/renderer/utils';
import {
  TranslatorProjectMetaData,
  TranslatorProject,
} from '@src/main/services/translator';
import { translatorProjectIpc, addTranslatorProjectDialog } from '@src/renderer/services';

/**
 * View: Translator Project
 */
@Component
export default class VTranslatorProject extends Vue {
  private loading: boolean = true;

  private path: string = '';

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  private project: TranslatorProject = null as any;

  private lock!: boolean;

  @Watch('project', { deep: true })
  private async watchProject(project: TranslatorProject): Promise<void> {
    await this.save();
  }

  private async load(): Promise<void> {
    const { path } = this.$route.query;
    console.log('path', path);
    const [, project] = await translatorProjectIpc.request('read', path as string);
    console.log('project', project);
    this.path = path as string;
    this.project = project;
  }

  private async save(): Promise<void> {
    if (this.lock) {
      this.lock = false;
      return;
    }
    translatorProjectIpc.send('change', { data: [this.path, this.project] });
  }

  private async beforeMount(): Promise<void> {
    translatorProjectIpc.on('change', (e, { data: [path, project] }) => {
      if (path === this.path) {
        this.lock = true;
        this.project = project;
      }
    });
    // TODO: unlink

    await this.load();
  }

  private beforeDestroy(): void {
    translatorProjectIpc.removeAllListener('change');
  }

  private render(h: CreateElement): VNode {
    const { path, project } = this;
    if (!project) {
      return <div staticClass="v-translator-project rw-container"></div>;
    }

    return (
      <div staticClass="v-translator-project rw-container">
        <div staticClass="v-translator-project_wrapper">
          <header staticClass="v-translator-project_header">
            <span staticClass="v-translator-project_category">Translation Project</span>
            <span staticClass="v-translator-project_name">{project.meta.name}</span>
            <span staticClass="v-translator-project_path">{path}</span>
          </header>
          <div staticClass="v-translator-project_mods"></div>
          <div staticClass="v-translator-project_languages"></div>
          <div staticClass="v-translator-project_options"></div>
        </div>
      </div>
    );
  }
}
