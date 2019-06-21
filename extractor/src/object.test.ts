import { replaceListItem, cloneObject } from './object';

describe('object', () => {
  test('replaceListItem', () => {
    const list = [{ value: '1' }, { value: '2' }];
    const [a, b] = list;
    const c = { value: '3' };
    const d = { value: '4' };

    replaceListItem(list, c, d);
    expect(list.length).toBe(2);
    expect(list.includes(c)).toBe(false);
    expect(list.includes(d)).toBe(false);

    replaceListItem(list, a, c);
    expect(list.length).toBe(2);
    expect(list[0]).toBe(c);

    replaceListItem(list, b, d);
    expect(list.length).toBe(2);
    expect(list[1]).toBe(d);
  });

  test('cloneObject', () => {
    const a = { value: '1', key: 'test', x: 'y' };
    const b = cloneObject(a);

    expect(a).not.toBe(b);
    expect(a).toEqual(b);
  });
});
