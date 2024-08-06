import {PaymentDto} from './PaymentDto';


export interface SubscriptionPaymentDto {
  id: string;
  startDate: string;
  endDate: string;
  payment: PaymentDto;
}
