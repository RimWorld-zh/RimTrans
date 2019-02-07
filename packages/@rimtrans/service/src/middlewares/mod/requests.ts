import { api, resolveApi } from '../utils-requests';
import { Languages } from './models';

/**
 * API for Core
 */
export const core = {
  about: async () => (await api.get<string>('/core/about')).data,
  defs: async () => (await api.get<Record<string, string>>('/core/defs')).data,
  languages: async () => (await api.get<Languages>('/core/languages')).data,
  languagesUpdateAll: async () =>
    (await api.get<void>('/core/languages-update-all')).data,
  langIcon: (name: string) => resolveApi(`/core/language/${name}/icon.png`),
};
