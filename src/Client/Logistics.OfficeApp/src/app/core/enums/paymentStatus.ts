import {EnumLike} from './enumLike';

export enum PaymentStatus {
  Pending,
  Paid,
}

export const PaymentStatusEnum: EnumLike = {
  Pending: {value: 0, description: 'Pending'},
  Paid: {value: 1, description: 'Paid'}
};
