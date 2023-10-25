import {PaymentMethod} from '@core/enums';

export interface CreateInvoice {
  loadId: string;
  customerId: string;
  paymentMethod: PaymentMethod;
  paymentAmount: number;
}
