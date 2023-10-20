import {PaymentFor, PaymentMethod} from '@core/enums';

export interface CreatePayment {
  method: PaymentMethod;
  amount: number;
  paymentFor: PaymentFor;
  comment?: string;
}
