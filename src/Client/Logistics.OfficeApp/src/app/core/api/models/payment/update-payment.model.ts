import { PaymentFor, PaymentMethodType, PaymentStatus } from "./enums";

export interface UpdatePaymentCommand {
  id: string;
  method?: PaymentMethodType;
  amount?: number;
  paymentFor?: PaymentFor;
  status?: PaymentStatus;
  notes?: string;
}
