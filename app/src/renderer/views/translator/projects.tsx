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
 * View: TranslatorProjects
 */
@Component
export default class VTranslatorProjects extends Vue {
  private loading: boolean = true;

  private projectMetaMap: Record<string, TranslatorProjectMetaData> = {};

  private projectSelectedMap: Record<string, boolean> = {};

  private async beforeMount(): Promise<void> {
    translatorProjectIpc.on('add', (e, { data: [path, { meta }] }) => {
      this.$set(this.projectMetaMap, path, meta);
      this.$set(this.projectSelectedMap, path, false);
    });
    translatorProjectIpc.on('unlink', (e, { data: path }) => {
      this.$delete(this.projectMetaMap, path);
      this.$delete(this.projectSelectedMap, path);
    });
    translatorProjectIpc.on('change', (e, { data: [path, { meta }] }) => {
      this.$set(this.projectMetaMap, path, meta);
    });

    console.log('projectMetaMap', 'request');
    const metaMap = await translatorProjectIpc.request('search', undefined);
    const selectedMap: Record<string, boolean> = {};
    Object.entries(metaMap).forEach(([path, meta]) => (selectedMap[path] = false));
    console.log('projectMetaMap', cloneObject(metaMap));
    this.projectMetaMap = metaMap;
    this.projectSelectedMap = selectedMap;
  }

  private beforeDestroy(): void {
    translatorProjectIpc.removeAllListener('add', 'unlink', 'change');
  }

  private async onAddProject(event: MouseEvent): Promise<void> {
    const payload = await addTranslatorProjectDialog(this);
    if (payload && payload.fileName) {
      const {
        $states: {
          paths: { translatorProjects, separator },
        },
      } = this;
      const path = [
        translatorProjects,
        `${payload.fileName}${EXT_NAME_TRANSLATOR_PROJECT}`,
      ].join(separator);
      const project: TranslatorProject = {
        meta: {
          name: payload.projectName,
          timeCreated: Date.now(),
          timeLastAccess: Date.now(),
          mods: [],
        },
        extractConfig: {
          modConfigs: [],
          languages: [],
        },
      };
      translatorProjectIpc.send('add', { data: [path, project] });
    }
  }

  private onOpenProjectsFolder(event: MouseEvent): void {
    const {
      $states: {
        paths: { translatorProjects },
      },
    } = this;
    interaction.openItem(translatorProjects);
  }

  private onSelectAll(event: MouseEvent): void {
    Object.keys(this.projectSelectedMap).forEach(
      path => (this.projectSelectedMap[path] = true),
    );
  }

  private onSelectInverse(event: MouseEvent): void {
    Object.entries(this.projectSelectedMap).forEach(
      ([path, selected]) => (this.projectSelectedMap[path] = !selected),
    );
  }

  private async onDeleteSelectedProjects(event: MouseEvent): Promise<void> {
    const paths = Object.entries(this.projectSelectedMap)
      .filter(([path, selected]) => selected)
      .map(([path, selected]) => path);

    const [confirm] = await this.$rw_showDialog(
      this,
      {
        title: 'Delete Projects',
        block: true,
        confirmLabel: 'Confirm',
        confirmColor: 'danger',
        cancelLabel: 'Cancel',
      },
      h => (
        <div>
          Detete projects?
          <ul>
            {paths.map(path => (
              <li>{this.projectMetaMap[path].name || path}</li>
            ))}
          </ul>
        </div>
      ),
    );

    if (confirm) {
      await Promise.all(
        paths.map(path => translatorProjectIpc.send('unlink', { data: path })),
      );
    }
  }

  private async deleteProject(
    path: string,
    meta: TranslatorProjectMetaData,
  ): Promise<void> {
    const [confirm] = await this.$rw_showDialog(this, {
      title: 'Delete Project',
      block: true,
      confirmLabel: 'Confirm',
      confirmColor: 'danger',
      cancelLabel: 'Cancel',
      content: `Delete the project "${meta.name || path}"?`,
    });
    if (confirm) {
      await translatorProjectIpc.send('unlink', { data: path });
    }
  }

  private render(h: CreateElement): VNode {
    const { projectMetaMap } = this;

    const pairs = Object.keys(projectMetaMap)
      .sort()
      .map<[string, TranslatorProjectMetaData]>(k => [k, projectMetaMap[k]]);

    return (
      <div staticClass="v-translator-projects">
        <div staticClass="v-translator-projects_wrapper rw-container">
          <div key="toolbar" staticClass="v-translator-projects_toolbar">
            <rw-button
              key="add"
              size="medium"
              shape="square"
              skin="flat"
              onClick={this.onAddProject}
              title="Add a project"
            >
              <md-icon staticClass="rw-button-icon" icon="FilePlusOutline"></md-icon>
            </rw-button>

            <rw-button
              key="open-folder"
              size="medium"
              shape="square"
              skin="flat"
              onClick={this.onOpenProjectsFolder}
              title="Open projects folder"
            >
              <md-icon staticClass="rw-button-icon" icon="FolderOpenOutline"></md-icon>
            </rw-button>

            <i staticClass="v-translator-projects_toolbar-blank"></i>

            <rw-button
              key="select-all"
              size="medium"
              shape="square"
              skin="flat"
              onClick={this.onSelectAll}
              title="Select All"
            >
              <md-icon staticClass="rw-button-icon" icon="SelectAll"></md-icon>
            </rw-button>

            <rw-button
              key="select-inverse"
              size="medium"
              shape="square"
              skin="flat"
              onClick={this.onSelectInverse}
              title="Select Inverse"
            >
              <md-icon staticClass="rw-button-icon" icon="SelectInverse"></md-icon>
            </rw-button>

            <rw-button
              key="delete"
              size="medium"
              shape="square"
              skin="flat"
              onClick={this.onDeleteSelectedProjects}
              title="Delete selected projects"
            >
              <md-icon staticClass="rw-button-icon" icon="DeleteOutline"></md-icon>
            </rw-button>
          </div>

          {pairs.map(([path, meta]) => (
            <div key={path} staticClass="v-translator-projects_item">
              <rw-checkbox vModel={this.projectSelectedMap[path]}>
                <span staticClass="sr-only">Select</span>
              </rw-checkbox>

              <router-link
                staticClass="v-translator-projects_item-label"
                to={`/translator/project?path=${path}`}
              >
                {meta.name || path}
              </router-link>

              <rw-button
                staticClass="v-translator-projects_item-button"
                color="default"
                size="medium"
                shape="square"
                skin="flat"
                onClick={() => this.deleteProject(path, meta)}
                title="Delete"
              >
                <md-icon staticClass="rw-button-icon" icon="DeleteOutline"></md-icon>
                <span staticClass="sr-only">Delete</span>
              </rw-button>
            </div>
          ))}
        </div>
      </div>
    );
  }
}
