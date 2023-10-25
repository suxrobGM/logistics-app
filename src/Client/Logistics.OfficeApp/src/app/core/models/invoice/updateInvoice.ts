import {PaymentMethod} from '@core/enums';

export interface UpdateInvoice {
  id: string;
  paymentMethod: PaymentMethod;
  paymentAmount: number;
}
