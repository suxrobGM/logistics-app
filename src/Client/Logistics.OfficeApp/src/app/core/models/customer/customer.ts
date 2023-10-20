import {Invoice} from '../payment/invoice';

export interface Customer {
  id: string;
  name: string;
  invoices: Invoice[];
}
