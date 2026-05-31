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
import {
  compositeValidators,
  validateRequired,
  validateKebabCase,
} from '@src/main/utils/validators';

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
    const {
      fileName,
      projectName,
      $states: {
        i18n: {
          translator: {
            projects: {
              addProject,
              addProjectDescription,
              fieldFileName,
              fieldProjectName,
            },
          },
        },
      },
    } = this;

    return (
      <div staticClass="s-add-translator-project-dialog">
        <rw-form>
          <rw-form-stack
            key="description"
            domPropsInnerHTML={addProjectDescription}
          ></rw-form-stack>

          <rw-form-stack key="field-file">
            <rw-text-field
              label={fieldFileName}
              value={fileName}
              onInput={(v: string) => this.$emit('fileNameChange', v)}
              validator={compositeValidators(validateRequired, validateKebabCase)}
            ></rw-text-field>
          </rw-form-stack>

          <rw-form-stack key="field-project">
            <rw-text-field
              label={fieldProjectName}
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
  fileName: string = '',
  projectName: string = '',
): Promise<AddTranslatorProjectPayload | undefined> {
  const {
    $states: {
      i18n: {
        dialog: { confirm: confirmLabel, cancel: cancelLabel },
        translator: {
          projects: { addProject },
        },
      },
    },
  } = parent;

  const [confirm, payload] = await parent.$rw_showDialog(
    parent,
    {
      title: addProject,
      block: true,
      confirmLabel,
      cancelLabel,
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
      fileName,
      projectName,
    }),
  );

  return (confirm && payload) || undefined;
}
