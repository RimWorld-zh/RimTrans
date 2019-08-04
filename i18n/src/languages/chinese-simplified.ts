import { LanguageData } from '../models';

export const ChineseSimplified: LanguageData = {
  info: {
    translators: [],

    languageID: 'ChineseSimplified',
    languageCodes: ['zh', 'zh-CN', 'zh-SG'],

    languageNameNative: '简体中文',
    languageNameEnglish: 'Simplified Chinese',
  },

  dict: {
    app: {
      name: '边缘译',
      description: '边缘世界（RimWorld）翻译与模组制作工具。',

      /** Names for tools */
      tool: {
        translator: '翻译器',
        modder: '模组制作器',
        translationWorkshop: '翻译工坊',
      },
    },

    common: {
      game: '游戏',
      mod: '模组', // singular
      mods: '模组', // plural
      steam: 'Steam',
      steamWorkshop: 'Steam 创意工坊',
      rimworld: '边缘世界',
    },

    modMeta: {
      detail: '详情',
      name: '名称',
      author: '作者',
      url: '链接',
      description: '说明',
      targetVersion: '目标版本',
      supportedVersions: '支持版本',
    },

    /** Dialog related */
    dialog: {
      ok: 'OK',
      yes: '是',
      no: '否',
      confirm: '确定',
      cancel: '取消',
    },

    /** Editor and Clipboard related */
    editor: {
      undo: '撤销',
      redo: '重做',
      cut: '剪切',
      copy: '复制',
      paste: '粘贴',
      selectAll: '全选',
    },

    /** File and Directory related */
    file: {
      file: '文件',
      folder: '文件夹',
      directory: '目录', // same as 'folder'

      path: '路径', // the path to a file or a directory
      explore: '浏览', // open a dialog to select file or directory

      view: '查看', // preview a text file or a image
      open: '打开', // open a file or directory
      close: '关闭', // close a file or directory
      closeAll: '全部关闭',

      add: '添加', // add a item to a list
      delete: '删除', // delete a item in a list, or delete file
      remove: '移除', // remove a item in a list, but not delete file

      load: '载入',
      save: '保存',
      saveAll: '全部保存',

      loading: '载入中', // Loading in process
    },

    /** Settings */
    settings: {
      /** Categories */
      features: '功能',
      ui: '用户界面',
      about: '关于',

      /** Features */
      directoryRimWorld: 'RimWorld 安装目录',
      directoryWorkshop: '创意工坊模组目录',

      /** UI */
      darkMode: '深色模式',
    },
  },
};
