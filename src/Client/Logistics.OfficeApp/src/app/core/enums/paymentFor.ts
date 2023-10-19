import {EnumLike} from './enumLike';

export enum PaymentFor {
  Payroll,
  Subscription,
  Invoice,
  Other
}

export const PaymentForEnum: EnumLike = {
  Payroll: {value: 0, description: 'Payroll'},
  Subscription: {value: 1, description: 'Subscription'},
  Invoice: {value: 2, description: 'Invoice'},
  Other: {value: 3, description: 'Other'}
};
