import {PaymentMethodType} from "@/core/enums";

export interface UpdateInvoiceCommand {
  id: string;
  paymentMethod: PaymentMethodType;
  paymentAmount: number;
}
