export type CSSStyle = Partial<CSSStyleDeclaration>;

export const GOLDEN_RATIO = (Math.sqrt(5) - 1) / 2;

export type Theme = 'light' | 'dark';

export type Color =
  | 'primary'
  | 'secondary'
  | 'blue'
  | 'teal'
  | 'magenta'
  | 'green'
  | 'yellow'
  | 'red'
  | 'purple'
  | 'orange';

export type ButtonColor = Color;
export type ButtonSize = 'small' | 'medium' | 'large';
export type ButtonSkin = 'fill' | 'silk' | 'flat' | 'plain';
export type ButtonShape = 'rect' | 'pill' | 'square' | 'circle';

export const MOTION_DURATION_BASE = 250;
export const MOTION_DURATION_1 = 100;
export const MOTION_DURATION_2 = 200;
export const MOTION_DURATION_3 = 300;
export const MOTION_DURATION_4 = 400;
