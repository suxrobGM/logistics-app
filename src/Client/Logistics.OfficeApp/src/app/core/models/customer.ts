import {Invoice} from './invoice';

export interface Customer {
  id: string;
  name: string;
  invoices: Invoice[];
}
