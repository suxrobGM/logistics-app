import {PaymentMethodType} from "@/core/enums";

export interface CreateInvoiceCommand {
  loadId: string;
  customerId: string;
  paymentMethod: PaymentMethodType;
  paymentAmount: number;
}
