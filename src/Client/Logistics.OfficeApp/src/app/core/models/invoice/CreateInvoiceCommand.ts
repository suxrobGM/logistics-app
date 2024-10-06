import {PaymentMethod} from '@/core/enums';

export interface CreateInvoiceCommand {
  loadId: string;
  customerId: string;
  paymentMethod: PaymentMethod;
  paymentAmount: number;
}
