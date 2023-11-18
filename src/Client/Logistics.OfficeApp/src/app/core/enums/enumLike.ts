export interface EnumValue {
  value: string | number;
  description: string;
}

export interface EnumLike {
  getDescription: GetDescriptionFn;
  [key: string]: EnumValue | GetDescriptionFn;
}

export function convertEnumToArray(enumLike: EnumLike): EnumValue[] {
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

export function getEnumDescription(enumLike: EnumLike, enumValue: string | number): string {
  for (const key in enumLike) {
    const item = enumLike[key];
    if (typeof item !== 'function' && item.value === enumValue) {
      return item.description;
    }
  }
  
  return 'Description not found';
}

type GetDescriptionFn = (enumValue: string | number) => string;
type ToArrayFn = (enumLike: EnumLike) => EnumValue[];
