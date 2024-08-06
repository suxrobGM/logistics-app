import {Payment} from './Payment_';


export interface SubscriptionPayment {
  id: string;
  startDate: string;
  endDate: string;
  payment: Payment;
}
