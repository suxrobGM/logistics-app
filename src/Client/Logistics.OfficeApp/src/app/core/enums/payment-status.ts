import {EnumLike, EnumType, convertEnumToArray, findValueFromEnum} from "./enum-like";

export enum PaymentStatus {
  Pending,
  Paid,
}

export const PaymentStatusEnum: EnumLike = {
  Pending: {value: 0, description: "Pending"},
  Paid: {value: 1, description: "Paid"},

  getValue(enumValue: string | number): EnumType {
    return findValueFromEnum(this, enumValue);
  },

  toArray(): EnumType[] {
    return convertEnumToArray(this);
  },
};
