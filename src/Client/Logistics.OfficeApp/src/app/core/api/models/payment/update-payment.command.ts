import {PaymentFor, PaymentMethodType, PaymentStatus} from "@/core/enums";

export interface UpdatePaymentCommand {
  id: string;
  method?: PaymentMethodType;
  amount?: number;
  paymentFor?: PaymentFor;
  status?: PaymentStatus;
  notes?: string;
}
