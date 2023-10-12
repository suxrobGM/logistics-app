import {Payment} from './payment';


export interface SubscriptionPayment {
  startDate: string;
  endDate: string;
  payment: Payment;
}
