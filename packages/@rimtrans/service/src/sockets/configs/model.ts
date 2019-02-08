/**
 * Client Configs
 */
export interface Configs {
  language: string;
}

export function newConfigs(): Configs {
  return {
    language: 'auto',
  };
}
