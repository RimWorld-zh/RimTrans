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
import { basename, extname } from 'path';
import { EXT_NAME_TRANSLATOR_PROJECT } from '@src/main/utils/constants';
import { cloneObject } from '@src/main/utils/object';
import { interaction } from '@src/renderer/utils';
import {
  TranslatorProject,
  TranslatorProjectMetaData,
} from '@src/main/services/translator';
import { translatorProjectIpc, addTranslatorProjectDialog } from '@src/renderer/services';

/**
 * View: TranslatorProjects
 */
@Component
export default class VTranslatorProjects extends Vue {
  private loading: boolean = true;

  private projectMap: Record<string, TranslatorProject> = {};

  private projectSelectedMap: Record<string, boolean> = {};

  private async load(): Promise<void> {
    console.log('projectMetaMap', 'request');
    const metaMap = await translatorProjectIpc.request('search', undefined);
    const selectedMap: Record<string, boolean> = {};
    Object.entries(metaMap).forEach(([path, meta]) => (selectedMap[path] = false));
    console.log('projectMetaMap', cloneObject(metaMap));
    this.projectMap = metaMap;
    this.projectSelectedMap = selectedMap;

    this.loading = false;
  }

  private async beforeMount(): Promise<void> {
    translatorProjectIpc.on('add', (e, { data: [path, project] }) => {
      this.$set(this.projectMap, path, project);
      this.$set(this.projectSelectedMap, path, false);
    });
    translatorProjectIpc.on('unlink', (e, { data: path }) => {
      this.$delete(this.projectMap, path);
      this.$delete(this.projectSelectedMap, path);
    });
    translatorProjectIpc.on('change', (e, { data: [path, project] }) => {
      this.$set(this.projectMap, path, project);
    });

    await this.load();
  }

  private beforeDestroy(): void {
    translatorProjectIpc.removeAllListener('add', 'unlink', 'change');
  }

  private async onAddProject(event: MouseEvent): Promise<void> {
    const existsFilenames = Object.keys(this.projectMap).map(p =>
      basename(p, extname(p)),
    );
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
    const {
      $states: {
        i18n: {
          dialog: { confirm: confirmLabel, cancel: cancelLabel },
          translator: {
            projects: { deleteProjects },
          },
        },
      },
    } = this;

    const paths = Object.entries(this.projectSelectedMap)
      .filter(([path, selected]) => selected)
      .map(([path, selected]) => path);

    const [confirm] = await this.$rw_showDialog(
      this,
      {
        title: deleteProjects,
        block: true,
        confirmLabel,
        confirmColor: 'danger',
        cancelLabel,
      },
      h => (
        <div>
          {paths.map(path => (
            <div>{this.projectMap[path].meta.name || path}</div>
          ))}
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
    const {
      $states: {
        i18n: {
          dialog: { confirm: confirmLabel, cancel: cancelLabel },
          translator: {
            projects: { deleteProject },
          },
        },
      },
    } = this;

    const [confirm] = await this.$rw_showDialog(this, {
      title: deleteProject,
      block: true,
      confirmLabel,
      confirmColor: 'danger',
      cancelLabel,
      content: meta.name || path,
    });
    if (confirm) {
      await translatorProjectIpc.send('unlink', { data: path });
    }
  }

  private render(h: CreateElement): VNode {
    const {
      projectMap,
      $states: {
        i18n: {
          form: { select, selectAll, selectInverse, deleteSelected },
          translator: {
            projects: { addProject, openProjectsFolder, deleteProject, deleteProjects },
          },
        },
      },
    } = this;

    const pairs = Object.keys(projectMap)
      .sort()
      .map<[string, TranslatorProjectMetaData]>(k => [k, projectMap[k].meta]);

    return (
      <div staticClass="v-translator-projects rw-container">
        <div staticClass="v-translator-projects_wrapper">
          <div key="toolbar" staticClass="v-translator-projects_toolbar">
            <rw-button
              key="add"
              size="medium"
              shape="square"
              skin="flat"
              onClick={this.onAddProject}
              title={addProject}
            >
              <md-icon staticClass="rw-button-icon" icon="FilePlusOutline"></md-icon>
            </rw-button>

            <rw-button
              key="open-folder"
              size="medium"
              shape="square"
              skin="flat"
              onClick={this.onOpenProjectsFolder}
              title={openProjectsFolder}
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
              title={selectAll}
            >
              <md-icon staticClass="rw-button-icon" icon="SelectAll"></md-icon>
            </rw-button>

            <rw-button
              key="select-inverse"
              size="medium"
              shape="square"
              skin="flat"
              onClick={this.onSelectInverse}
              title={selectInverse}
            >
              <md-icon staticClass="rw-button-icon" icon="SelectInverse"></md-icon>
            </rw-button>

            <rw-button
              key="delete"
              size="medium"
              shape="square"
              skin="flat"
              onClick={this.onDeleteSelectedProjects}
              title={deleteSelected}
            >
              <md-icon staticClass="rw-button-icon" icon="DeleteOutline"></md-icon>
            </rw-button>
          </div>

          <div key="list" staticClass="v-translator-projects_list">
            {pairs.map(([path, meta]) => (
              <div key={path} staticClass="v-translator-projects_item">
                <rw-checkbox vModel={this.projectSelectedMap[path]}>
                  <span staticClass="sr-only">Select</span>
                </rw-checkbox>

                <router-link
                  staticClass="v-translator-projects_item-label"
                  to={`/translator/project?path=${path}`}
                >
                  {meta.name}
                  <span staticClass="v-translator-projects_item-sub-label">
                    {basename(path)}
                  </span>
                </router-link>

                <rw-button
                  staticClass="v-translator-projects_item-button"
                  color="default"
                  size="medium"
                  shape="square"
                  skin="flat"
                  onClick={() => this.deleteProject(path, meta)}
                  title={deleteProject}
                >
                  <md-icon staticClass="rw-button-icon" icon="DeleteOutline"></md-icon>
                  <span staticClass="sr-only">Delete</span>
                </rw-button>
              </div>
            ))}
          </div>
        </div>
      </div>
    );
  }
}
