import { PaymentFor, PaymentMethodType } from "./enums";

export interface CreatePaymentCommand {
  method: PaymentMethodType;
  amount: number;
  paymentFor: PaymentFor;
  notes?: string;
}
