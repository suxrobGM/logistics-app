import {CustomerDto} from '../customer/CustomerDto';
import {Payment} from '../payment/Payment_';

export interface InvoiceDto {
  id: string;
  loadId: string;
  loadRef: number;
  createdDate: string;
  customer: CustomerDto;
  payment: Payment;
}
