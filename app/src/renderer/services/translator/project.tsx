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
import { IPC_NAMESPACE_TRANSLATOR_PROJECT } from '@src/main/utils/constants';
import {
  TranslatorProject,
  TranslatorProjectMetaData,
  TranslatorProjectIpcTypeMap,
} from '@src/main/services/translator';
import { createIpc } from '@src/renderer/utils';

export const translatorProjectIpc = createIpc<TranslatorProjectIpcTypeMap>(
  IPC_NAMESPACE_TRANSLATOR_PROJECT,
);

/**
 * Component: Translator Projects
 */
@Component
export class STranslatorProjects extends Vue {
  private render(h: CreateElement): VNode {
    return <div staticClass="s-translator-projects">{this.$slots.default}</div>;
  }
}

/**
 * Component: Translator Projects
 */
@Component
export class STranslatorProject extends Vue {
  private render(h: CreateElement): VNode {
    return <div staticClass="s-translator-project">{this.$slots.default}</div>;
  }
}
