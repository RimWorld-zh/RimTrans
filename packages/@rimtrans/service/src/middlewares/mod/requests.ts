import { api } from '../utils-requests';

/**
 * API for Core
 */
export const core = {
  about: async () => (await api.get<string>('/core/about')).data,
  defs: async () => (await api.get<Record<string, string>>('/core/defs')).data,
  languages: async () => (await api.get<Record<string, string>>('/core/languages')).data,
};
