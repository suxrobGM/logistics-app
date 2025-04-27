import {PaymentMethodType} from "../payment";

export interface UpdateInvoiceCommand {
  id: string;
  paymentMethod: PaymentMethodType;
  paymentAmount: number;
}
