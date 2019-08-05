import Vue, { CreateElement, VNode } from 'vue';
import { ScopedSlotReturnValue } from 'vue/types/vnode';
import {
  Component,
  Emit,
  Inject,
  Model,
  Prop,
  Provide,
  Watch,
} from 'vue-property-decorator';
import { ModMetaData } from '@rimtrans/extractor';
import { SortDirection, sort, clone } from '@src/main/utils/object';
import { ChoiceOption, ChoiceSlotProps } from '@src/renderer/components';

type Genre = 'local' | 'steam';

const sortProperties: (keyof ModMetaData)[] = ['path', 'name', 'author'];

/**
 * Component: SelectModDialog
 */
@Component
export class SSelectModDialog extends Vue {
  public loading: boolean = false;

  public genre: Genre = 'local';

  public lastSortBy: keyof ModMetaData = 'path';

  public lastSortDirection: SortDirection = 'ASC';

  public modsLocal: Record<string, ModMetaData> = {};

  public modsLocalOptions: ChoiceOption[] = [];

  @Prop({ type: Array, required: true })
  public modsLocalSelected!: string[];

  private onLocalChange(value: string[]): void {
    this.$emit('localChange', value);
  }

  public modsSteam: Record<string, ModMetaData> = {};

  public modsSteamOptions: ChoiceOption[] = [];

  @Prop({ type: Array, required: true })
  public modsSteamSelected!: string[];

  private onSteamChange(value: string[]): void {
    this.$emit('steamChange', value);
  }

  private convert(
    mods: Record<string, ModMetaData>,
    sortBy: keyof ModMetaData,
    sortDirection: SortDirection,
  ): ChoiceOption[] {
    return sort(
      Object.values(mods),
      [...new Set([sortBy, ...sortProperties])],
      sortDirection,
    ).map<ChoiceOption>(meta => ({
      value: meta.path,
    }));
  }

  private async loadMods(): Promise<void> {
    this.loading = true;
    this.modsLocal = {};
    this.modsSteam = {};
    this.modsLocalOptions = [];
    this.modsSteamOptions = [];

    const { lastSortBy, lastSortDirection } = this;

    const [modsLocal, modsSteam] = await Promise.all([
      this.$ipc.request('mod-meta-data', 'local'),
      this.$ipc.request('mod-meta-data', 'steam'),
    ]);

    const [modsLocalOptions, modsSteamOptions] = [modsLocal, modsSteam].map(mods =>
      this.convert(mods, lastSortBy, lastSortDirection),
    );

    this.modsLocal = modsLocal;
    this.modsSteam = modsSteam;
    this.modsLocalOptions = modsLocalOptions;
    this.modsSteamOptions = modsSteamOptions;
    this.loading = false;
  }

  private sort(sortBy: keyof ModMetaData): void {
    const { modsLocal, modsSteam, lastSortBy, lastSortDirection } = this;
    const sortDirection: SortDirection =
      (sortBy === lastSortBy && ((lastSortDirection === 'ASC' && 'DESC') || 'ASC')) ||
      'ASC';

    this.lastSortBy = sortBy;
    this.lastSortDirection = sortDirection;
    this.modsLocalOptions = this.convert(clone(modsLocal), sortBy, sortDirection);
    this.modsSteamOptions = this.convert(clone(modsSteam), sortBy, sortDirection);
  }

  private mounted(): void {
    this.loadMods();
  }

  private render(h: CreateElement): VNode {
    const {
      loading,
      genre,
      modsLocal,
      modsLocalOptions,
      modsLocalSelected,
      modsSteam,
      modsSteamOptions,
      modsSteamSelected,
      lastSortBy,
      lastSortDirection,
    } = this;

    const mods = (genre === 'local' && modsLocal) || modsSteam;

    const itemOuter = ({ value: path }: ChoiceSlotProps): ScopedSlotReturnValue => {
      const meta = mods[path];
      const { id = undefined, name = undefined, author = undefined } =
        (typeof meta === 'object' && meta) || {};
      return [
        <span staticClass="s-select-mod-dialog_cell" title={path}>
          {id}
        </span>,
        <span staticClass="s-select-mod-dialog_cell">{name}</span>,
        <span staticClass="s-select-mod-dialog_cell is-clamp is-secondary" title={author}>
          {author}
        </span>,
      ];
    };

    return (
      <div staticClass="s-select-mod-dialog">
        <header key="header" staticClass="s-select-mod-dialog_header">
          <rw-button
            key="local"
            disabled={loading}
            color={(genre === 'local' && 'primary') || 'default'}
            onClick={() => (this.genre = 'local')}
          >
            <mdi staticClass="rw-button-icon rw-icon-left" icon="FolderOutline" />
            {this.$i18n.dict.common.mods}
          </rw-button>
          <rw-button
            key="steam"
            disabled={loading}
            color={(genre === 'steam' && 'primary') || 'default'}
            onClick={() => (this.genre = 'steam')}
          >
            <mdi staticClass="rw-button-icon rw-icon-left" icon="Steam" />
            {this.$i18n.dict.common.steam}
          </rw-button>

          <rw-button
            key="reload"
            disabled={loading}
            color="primary"
            shape="square"
            onClick={this.loadMods}
          >
            <mdi staticClass="rw-button-icon" icon="Reload" spin={loading} />
          </rw-button>
        </header>

        <menu key="menu" staticClass="s-select-mod-dialog_menu">
          <span key="label" staticClass="s-select-mod-dialog_menu-label">
            Sort By:
          </span>
          <rw-button
            key="path"
            disabled={loading}
            skin={(lastSortBy === 'path' && 'fill') || 'flat'}
            onClick={() => this.sort('path')}
          >
            {this.$i18n.dict.file.path}
            <mdi
              staticClass="rw-button-icon rw-icon-right"
              icon={
                (lastSortBy !== 'path' && 'SortVariant') ||
                (lastSortDirection === 'ASC' && 'SortAscending') ||
                'SortDescending'
              }
            />
          </rw-button>
          <rw-button
            key="name"
            disabled={loading}
            skin={(lastSortBy === 'name' && 'fill') || 'flat'}
            onClick={() => this.sort('name')}
          >
            {this.$i18n.dict.modMeta.name}
            <mdi
              staticClass="rw-button-icon rw-icon-right"
              icon={
                (lastSortBy !== 'name' && 'SortVariant') ||
                (lastSortDirection === 'ASC' && 'SortAscending') ||
                'SortDescending'
              }
            />
          </rw-button>
          <rw-button
            key="author"
            disabled={loading}
            skin={(lastSortBy === 'author' && 'fill') || 'flat'}
            onClick={() => this.sort('author')}
          >
            {this.$i18n.dict.modMeta.author}
            <mdi
              staticClass="rw-button-icon rw-icon-right"
              icon={
                (lastSortBy !== 'author' && 'SortVariant') ||
                (lastSortDirection === 'ASC' && 'SortAscending') ||
                'SortDescending'
              }
            />
          </rw-button>
        </menu>

        {(genre === 'local' && (
          <div key={`mods-local`} staticClass="s-select-mod-dialog_scroll">
            <rw-multi-choice-group
              value={modsLocalSelected}
              onChange={this.onLocalChange}
              options={modsLocalOptions}
              wrapperClass="s-select-mod-dialog_wrapper"
              {...{ scopedSlots: { itemOuter } }}
            />
          </div>
        )) || (
          <div key={`mods-steam`} staticClass="s-select-mod-dialog_scroll">
            <rw-multi-choice-group
              value={modsSteamSelected}
              onChange={this.onSteamChange}
              options={modsSteamOptions}
              wrapperClass="s-select-mod-dialog_wrapper"
              {...{ scopedSlots: { itemOuter } }}
            />
          </div>
        )}
      </div>
    );
  }
}

export async function selectModDialog(parent: Vue): Promise<string[] | undefined> {
  return new Promise<string[] | undefined>((resolve, reject) => {
    const state = new Vue({
      parent,
      data() {
        return {
          modsLocalSelected: [] as string[],
          modsSteamSelected: [] as string[],
        };
      },
    });

    parent
      .$rw_showDialog(
        {
          title: 'Select Mods',
          block: true,
          confirmLabel: 'Confirm',
          cancelLabel: 'Cancel',
        },
        parent,
        h =>
          h(SSelectModDialog, {
            props: {
              modsLocalSelected: state.modsLocalSelected,
              modsSteamSelected: state.modsSteamSelected,
            },
            on: {
              localChange: (v: string[]) => (state.modsLocalSelected = v),
              steamChange: (v: string[]) => (state.modsSteamSelected = v),
            },
          }),
      )
      .then(confirm => {
        const result =
          (confirm && [...state.modsLocalSelected, ...state.modsSteamSelected]) ||
          undefined;
        state.$destroy();
        parent.$nextTick(() => resolve(result));
      });
  });
}
