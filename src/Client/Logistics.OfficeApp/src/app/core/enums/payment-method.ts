import {EnumLike, EnumType, convertEnumToArray, findValueFromEnum} from "./enum-like";

export enum PaymentMethodType {
  BankAccount,
  CreditCard,
  Cash,
}

export const PaymentMethodTypeEnum: EnumLike = {
  BankAccount: {value: 0, description: "Bank Account"},
  CreditCard: {value: 1, description: "Credit Card"},
  Cash: {value: 2, description: "Cash"},

  getValue(enumValue: string | number): EnumType {
    return findValueFromEnum(this, enumValue);
  },

  toArray(): EnumType[] {
    return convertEnumToArray(this);
  },
};
