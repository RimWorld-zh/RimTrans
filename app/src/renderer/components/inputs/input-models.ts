export interface ChoiceOption<T extends string | number = string> {
  value: T;
  label?: string;
  disabled?: boolean;
}

export interface ChoiceSlotProps<T extends string | number = string>
  extends ChoiceOption<T> {
  index: number;
  checked: boolean;
}
