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
import { prop, when } from '../../base';

/**
 * Component: Form Label
 */
@Component
export class RwFormLabel extends Vue {
  private render(h: CreateElement): VNode {
    return <span staticClass="rw-form-label">{this.$slots.default}</span>;
  }
}

/**
 * Component: Form Field
 */
@Component
export class RwFormFieldGroup extends Vue {
  private render(h: CreateElement): VNode {
    return <div staticClass="rw-form-field-group">{this.$slots.default}</div>;
  }
}

/**
 * Component: Form Stack
 */
@Component
export class RwFormStack extends Vue {
  private render(h: CreateElement): VNode {
    return <div staticClass="rw-form-row">{this.$slots.default}</div>;
  }
}

/**
 * Component: Form
 */
@Component
export class RwForm extends Vue {
  private render(h: CreateElement): VNode {
    return <form staticClass="rw-form">{this.$slots.default}</form>;
  }
}
