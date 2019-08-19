export type CSSStyle = Partial<CSSStyleDeclaration>;

export const GOLDEN_RATIO = (Math.sqrt(5) - 1) / 2;

export type Color =
  | 'green'
  | 'blue'
  | 'purple'
  | 'violet'
  | 'magenta'
  | 'red'
  | 'orange'
  | 'yellow';
export type ColorTheme = 'light' | 'dark';
export type ColorSemantic = 'primary' | 'secondary' | 'success' | 'warning' | 'danger';

export type Size = 'small' | 'medium' | 'large';

export type Position = 'top' | 'bottom' | 'left' | 'right';

export type ButtonColor = Color | ColorSemantic;
export type ButtonSize = Size;
export type ButtonSkin = 'fill' | 'silk' | 'flat' | 'plain';
export type ButtonShape = 'rect' | 'pill' | 'square' | 'circle';

export type SpinnerColor = Color | ColorSemantic;
export type SpinnerSize = Size;
export type SpinnerLabelPosition = Position;

export const MOTION_DURATION_BASE = 250;
export const MOTION_DURATION_1 = 100;
export const MOTION_DURATION_2 = 200;
export const MOTION_DURATION_3 = 300;
export const MOTION_DURATION_4 = 400;
