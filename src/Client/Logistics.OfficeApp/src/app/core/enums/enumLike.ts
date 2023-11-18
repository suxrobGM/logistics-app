export interface EnumType {
  value: string | number;
  description: string;
}

export interface EnumLike {
  getValue: GetValueFn;
  toArray: ToArrayFn;
  [key: string]: EnumType | GetValueFn | ToArrayFn;
}

export function convertEnumToArray(enumLike: EnumLike): EnumType[] {
  return Object.keys(enumLike)
    .filter(key => typeof enumLike[key] !== 'function') // Filter out function properties
    .map(key => {
      const enumItem = enumLike[key];

      if (typeof enumItem === 'function') {
        throw new Error('Encountered a function when converting enum to array');
      }

      return {
        value: enumItem.value,
        description: enumItem.description
      };
    });
}

export function findValueFromEnum(enumLike: EnumLike, enumValue: string | number): EnumType {
  for (const key in enumLike) {
    const item = enumLike[key];
    if (typeof item !== 'function' && item.value === enumValue) {
      return item;
    }
  }
  
  return {value: '', description: ''};
}

type GetValueFn = (enumValue: string | number) => EnumType;
type ToArrayFn = () => EnumType[];
