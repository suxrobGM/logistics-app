import { PaymentMethodType } from "../payment";

export interface CreateLoadInvoiceCommand {
  loadId: string;
  customerId: string;
  paymentMethod: PaymentMethodType;
  paymentAmount: number;
}
