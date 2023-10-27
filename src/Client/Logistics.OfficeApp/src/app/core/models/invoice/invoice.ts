import {Customer} from '../customer/customer';
import {Payment} from '../payment/payment';

export interface Invoice {
  loadId: string;
  loadRef: number;
  createdDate: string;
  customer: Customer;
  payment: Payment;
}
