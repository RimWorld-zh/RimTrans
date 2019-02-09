/**
 * Localization for English
 */
export const en = {
  code: 'en',
  name: 'English',
  label: 'English',
  translators: ['duduluu'],
  dict: {
    // common
    language: 'Language',
    latest_update: 'Latest Update',
    update: 'Update',
    update_all: 'Update All',

    // configs
    configs: 'Configs',
    configs_interface_languages: 'Interface Languages',
    configs_application: 'Application',
    configs_application_path_rimworld: 'Path to RimWorld directory',
    configs_application_path_workshop: 'Path to Workshop directory',
    configs_application_path_custom: 'Path to custom directory',
    // this 'Core' means RimWorld/Mods/Core, reserve it
    configs_core_languages: 'Core Languages',
  },
};

export type LocaleInfo = typeof en;
