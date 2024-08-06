import {PaymentFor, PaymentMethod, PaymentStatus} from '@core/enums';

export interface UpdatePaymentCommand {
  id: string;
  method?: PaymentMethod;
  amount?: number;
  paymentFor?: PaymentFor;
  status?: PaymentStatus;
  comment?: string;
}
