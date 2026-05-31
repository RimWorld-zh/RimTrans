export interface Storage {
  lastActiveWindowState: {
    maximized: boolean;
    width: number | null;
    height: number | null;
    x: number | null;
    y: number | null;
  };
}

export function defaultStorage(): Storage {
  return {
    lastActiveWindowState: {
      maximized: false,
      width: null,
      height: null,
      x: null,
      y: null,
    },
  };
}
