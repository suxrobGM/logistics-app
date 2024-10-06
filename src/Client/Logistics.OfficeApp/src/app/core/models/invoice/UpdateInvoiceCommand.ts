import {PaymentMethod} from '@/core/enums';

export interface UpdateInvoiceCommand {
  id: string;
  paymentMethod: PaymentMethod;
  paymentAmount: number;
}
