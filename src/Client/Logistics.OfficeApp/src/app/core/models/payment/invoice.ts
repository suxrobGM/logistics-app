import {Payment} from './payment';


export interface Invoice {
  id: string;
  companyName?: string;
  companyAddress?: string;
  loadId: string;
  customerId: string;
  payment: Payment;
}
