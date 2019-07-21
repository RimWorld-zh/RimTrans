export interface Settings {
  language: string;
}

export function defaultSettings(): Settings {
  return {
    language: 'auto',
  };
}
