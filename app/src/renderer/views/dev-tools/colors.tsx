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
import { Color } from '@src/renderer/components/base';

const colors: Color[] = [
  'green',
  'blue',
  'purple',
  'violet',
  'magenta',
  'red',
  'orange',
  'yellow',
];
const levels = Array(10)
  .fill(0)
  .map((_, index) => (index === 0 ? `05` : `${index * 10}`));
const alphas = Array(9)
  .fill(0)
  .map((_, index) => (index + 1) * 10);

/**
 * Component: DevToolsColors
 */
@Component
export default class VDevToolsColors extends Vue {
  private alpha: number = 0;

  private render(h: CreateElement): VNode {
    return (
      <div staticClass="v-dev-tools-colors">
        <section key="master">
          <h1 staticClass="v-dev-tools-colors_title" key="title">
            Color Palette
          </h1>
          {colors.map(name => (
            <div key={name} staticClass="v-dev-tools-colors_row">
              {levels.map(lv => (
                <span
                  key={lv}
                  staticClass="v-dev-tools-colors_item"
                  style={{ backgroundColor: `var(--color-${name}-${lv})` }}
                />
              ))}
              <span staticClass="v-dev-tools-colors_label">{name}</span>
            </div>
          ))}
        </section>

        {colors.map(name => (
          <section key={name}>
            <h2 staticClass="v-dev-tools-colors_title" key="title">
              {name}
            </h2>
            <div>
              {levels.map(lv => (
                <div key={lv} staticClass="v-dev-tools-colors_row">
                  {alphas.map(a => (
                    <span
                      key={a}
                      staticClass="v-dev-tools-colors_item"
                      style={{ backgroundColor: `var(--color-${name}-${lv}-a${a})` }}
                    />
                  ))}
                  <span
                    key="raw"
                    staticClass="v-dev-tools-colors_item"
                    style={{ backgroundColor: `var(--color-${name}-${lv})` }}
                  />
                  <span staticClass="v-dev-tools-colors_label">{lv}</span>
                </div>
              ))}
            </div>
          </section>
        ))}
      </div>
    );
  }
}
