import {Payment} from './payment';


export interface SubscriptionPayment {
  id: string;
  startDate: string;
  endDate: string;
  payment: Payment;
}
