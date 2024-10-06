import {EnumLike, EnumType, convertEnumToArray, findValueFromEnum} from "./enum-like";

export enum PaymentFor {
  Payroll,
  Subscription,
  Invoice,
  Other,
}

export const PaymentForEnum: EnumLike = {
  Payroll: {value: 0, description: "Payroll"},
  Subscription: {value: 1, description: "Subscription"},
  Invoice: {value: 2, description: "Invoice"},
  Other: {value: 3, description: "Other"},

  getValue(enumValue: string | number): EnumType {
    return findValueFromEnum(this, enumValue);
  },

  toArray(): EnumType[] {
    return convertEnumToArray(this);
  },
};
