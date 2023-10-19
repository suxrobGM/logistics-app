export interface EnumValue {
  value: string | number;
  description: string;
}

export interface EnumLike {
  [key: string]: EnumValue;
}

export function convertEnumToArray(enumLike: EnumLike): EnumValue[] {
  return Object.keys(enumLike).map(key => {
    return {
      value: enumLike[key].value,
      description: enumLike[key].description
    };
  });
}

export function getEnumDescription(enumLike: EnumLike, enumValue: string | number): string {
  const enumItem = Object.values(enumLike).find(i => i.value === enumValue);
  return enumItem ? enumItem.description : 'Description not found';
}

