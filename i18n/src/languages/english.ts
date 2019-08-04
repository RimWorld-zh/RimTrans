import { LanguageData } from '../models';

export const English: LanguageData = {
  info: {
    translators: [],

    languageID: 'English',
    languageCodes: ['en'],

    languageNameNative: 'English',
    languageNameEnglish: 'English',
  },

  dict: {
    app: {
      name: 'RimTrans',
      description: 'A translation and modding tool for RimWorld.',

      /** Names for tools */
      tool: {
        translator: 'Translator',
        modder: 'Modder',
        translationWorkshop: 'Translation Workshop',
      },
    },

    common: {
      game: 'Game',
      mod: 'Mod', // singular
      mods: 'Mods', // plural
      steam: 'Steam',
      steamWorkshop: 'Steam Workshop',
      rimworld: 'RimWorld',
    },

    modMeta: {
      detail: 'Detail',
      name: 'Name',
      author: 'Author',
      url: 'Link',
      description: 'Description',
      targetVersion: 'Target Version',
      supportedVersions: 'Supported Version',
    },

    /** Dialog related */
    dialog: {
      ok: 'OK',
      yes: 'Yes',
      no: 'No',
      confirm: 'Confirm',
      cancel: 'Cancel',
    },

    /** Editor and Clipboard related */
    editor: {
      undo: 'Undo',
      redo: 'Redo',
      cut: 'Cut',
      copy: 'Copy',
      paste: 'Paste',
      selectAll: 'Select All',
    },

    /** File and Directory related */
    file: {
      file: 'File',
      folder: 'Folder',
      directory: 'Directory', // same as 'folder'

      path: 'Path', // the path to a file or a directory
      explore: 'Explore', // open a dialog to select file or directory

      view: 'View', // preview a text file or a image
      open: 'Open', // open a file or directory
      close: 'Close', // close a file or directory
      closeAll: 'Close All',

      add: 'Add', // add a item to a list
      delete: 'Delete', // delete a item in a list, or delete file
      remove: 'Remove', // remove a item in a list, but not delete file

      load: 'Load',
      save: 'Save',
      saveAll: 'Save All',

      loading: 'Loading', // Loading in process
    },

    /** Settings */
    settings: {
      /** Categories */
      features: 'Features',
      ui: 'User Interface',
      about: 'About',

      /** Features */
      directoryRimWorld: 'RimWorld Installation Directory',
      directoryWorkshop: 'Steam Workshop Mods Directory',

      /** UI */
      darkMode: 'Dark Mode',
    },
  },
};
