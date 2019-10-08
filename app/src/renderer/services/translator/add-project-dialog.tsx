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
import { validateKebabCase } from '@src/main/utils/validators';

export interface AddTranslatorProjectPayload {
  fileName: string;
  projectName: string;
}

/**
 * Component: Add Translator Project Dialog
 */
@Component
export class SAddTranslatorProjectDialog extends Vue
  implements AddTranslatorProjectPayload {
  @Prop({ type: String, required: true })
  public readonly fileName!: string;

  @Prop({ type: String, required: true })
  public readonly projectName!: string;

  private render(h: CreateElement): VNode {
    const { fileName, projectName } = this;

    return (
      <div staticClass="s-add-translator-project-dialog">
        <rw-form>
          <rw-form-stack
            key="description"
            domPropsInnerHTML={`The <strong>"file name"</strong> field contains your project's file name, and 
                                must be lower case English word or number, and may contain single hyphen
                                between each word.
                                <br />
                                The <strong>"project name"</strong> field is only for display, set what you
                                like.`}
          ></rw-form-stack>

          <rw-form-stack key="field-file">
            <rw-text-field
              label="file name"
              value={fileName}
              onInput={(v: string) => this.$emit('fileNameChange', v)}
              validator={validateKebabCase}
            ></rw-text-field>
          </rw-form-stack>

          <rw-form-stack key="field-project">
            <rw-text-field
              label="project name"
              value={projectName}
              onInput={(v: string) => this.$emit('projectNameChange', v)}
            ></rw-text-field>
          </rw-form-stack>
        </rw-form>
      </div>
    );
  }
}

export async function addTranslatorProjectDialog(
  parent: Vue,
): Promise<AddTranslatorProjectPayload | undefined> {
  const [confirm, payload] = await parent.$rw_showDialog(
    parent,
    {
      title: 'Add Project',
      block: true,
      confirmLabel: 'Confirm',
      cancelLabel: 'Cancel',
    },
    (h, state) =>
      h(SAddTranslatorProjectDialog, {
        props: {
          fileName: state.fileName,
          projectName: state.projectName,
        },
        on: {
          fileNameChange: (v: string) => (state.fileName = v),
          projectNameChange: (v: string) => (state.projectName = v),
        },
      }),
    (): AddTranslatorProjectPayload => ({
      fileName: '',
      projectName: '',
    }),
  );

  return (confirm && payload) || undefined;
}
