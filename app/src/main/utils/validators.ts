const REGEX_KEBAB_CASE = /^((\w+)|(\w+(-\w+)+))$/;

export function compositeValidators(
  ...validators: ((text: string) => boolean)[]
): (text: string) => boolean {
  return (text: string): boolean => {
    for (const validate of validators) {
      const result = validate(text);
      if (typeof result !== 'boolean') {
        return result;
      }
      if (!result) {
        return result;
      }
    }
    return true;
  };
}

export function validateRequired(text: string): boolean {
  return text.length > 0;
}

export function validateKebabCase(text: string): boolean {
  return REGEX_KEBAB_CASE.test(text) && text.toLowerCase() === text;
}
