import {PaymentFor, PaymentMethodType} from "@/core/enums";

export interface CreatePaymentCommand {
  method: PaymentMethodType;
  amount: number;
  paymentFor: PaymentFor;
  comment?: string;
}
