export function replaceListItem<T>(list: T[], oldItem: T, newItem: T): void {
  const index = list.indexOf(oldItem);
  if (index > -1) {
    list[index] = newItem;
  }
}

export function cloneObject<T extends object>(obj: T): T {
  return JSON.parse(JSON.stringify(obj));
}
