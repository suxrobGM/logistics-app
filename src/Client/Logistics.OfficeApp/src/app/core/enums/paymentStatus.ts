import {EnumLike} from './enumLike';

export enum PaymentStatus {
  Pending,
  Paid,
}

export const PaymentStatusEnum: EnumLike = {
  Pending: {value: '0', description: 'Super Admin'},
  Paid: {value: '1', description: 'Admin'}
};
