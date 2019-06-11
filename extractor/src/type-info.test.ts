import coreTypeInfo from '@rimtrans/type-info';

describe('type-info', () => {
  test('core type info', () => {
    // console.log(coreTypeInfo);

    expect(Array.isArray(coreTypeInfo.classes)).toBe(true);
    expect(Array.isArray(coreTypeInfo.enums)).toBe(true);
    expect(coreTypeInfo.classes.length > 0).toBe(true);
    const def = coreTypeInfo.classes[0];
    expect(def.name).toBe('Def');
    expect(def.fields[0].name).toBe('defName');
  });
});
