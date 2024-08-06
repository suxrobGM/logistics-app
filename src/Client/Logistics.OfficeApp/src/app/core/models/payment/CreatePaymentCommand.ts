import {PaymentFor, PaymentMethod} from '@core/enums';

export interface CreatePaymentCommand {
  method: PaymentMethod;
  amount: number;
  paymentFor: PaymentFor;
  comment?: string;
}
