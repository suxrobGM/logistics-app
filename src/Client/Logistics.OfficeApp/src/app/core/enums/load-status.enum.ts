import {EnumLike, EnumType, convertEnumToArray, findValueFromEnum} from "./enum-like";

export enum LoadStatus {
  Dispatched,
  PickedUp,
  Delivered,
}

export const LoadStatusEnum: EnumLike = {
  Dispatched: {value: 0, description: "Dispatched"},
  PickedUp: {value: 1, description: "Picked Up"},
  Delivered: {value: 2, description: "Delivered"},

  getValue(enumValue: string | number): EnumType {
    return findValueFromEnum(this, enumValue);
  },

  toArray(): EnumType[] {
    return convertEnumToArray(this);
  },
};
