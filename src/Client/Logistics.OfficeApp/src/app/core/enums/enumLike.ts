export interface EnumValue {
  value: string | number;
  description: string;
}

export interface EnumLike {
  getDescription: GetEnumDescFunction;
  [key: string]: EnumValue | GetEnumDescFunction;
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
  // Type assertion here to indicate to TypeScript that we are dealing with non-function items only
  const enumItem = Object.values(enumLike)
    .filter((item): item is EnumValue => typeof item !== 'function')
    .find(item => item.value === enumValue);
  
  return enumItem ? enumItem.description : 'Description not found';
}

type GetEnumDescFunction = (enumValue: string | number) => string;
