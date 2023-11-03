import {EnumLike, getEnumDescription} from './enumLike';

export enum PaymentMethod {
  BankAccount,
  CreditCard,
  Cash
}

export const PaymentMethodEnum: EnumLike = {
  BankAccount: {value: 0, description: 'Bank Account'},
  CreditCard: {value: 1, description: 'Credit Card'},
  Cash: {value: 2, description: 'Cash'},

  getDescription(value: string | number): string {
    return getEnumDescription(this, value);
  },
};
