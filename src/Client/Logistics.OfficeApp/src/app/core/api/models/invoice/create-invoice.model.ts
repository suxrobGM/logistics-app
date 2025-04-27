import {PaymentMethodType} from "../payment";

export interface CreateInvoiceCommand {
  loadId: string;
  customerId: string;
  paymentMethod: PaymentMethodType;
  paymentAmount: number;
}
