import {Customer} from '../customer/customer';
import {Payment} from '../payment/Payment_';

export interface Invoice {
  id: string;
  loadId: string;
  loadRef: number;
  createdDate: string;
  customer: Customer;
  payment: Payment;
}
