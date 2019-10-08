const REGEX_KEBAB_CASE = /^((\w+)|(\w+(-\w+)+))$/;

export function validateKebabCase(text: string): boolean {
  return REGEX_KEBAB_CASE.test(text) && text.toLowerCase() === text;
}
