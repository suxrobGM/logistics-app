import {PaymentDto} from "./payment.dto";

export interface SubscriptionPaymentDto {
  id: string;
  startDate: string;
  endDate: string;
  payment: PaymentDto;
}
