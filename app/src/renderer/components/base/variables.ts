export type CSSStyle = Partial<CSSStyleDeclaration>;

export const GOLDEN_RATIO = (Math.sqrt(5) - 1) / 2;

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
