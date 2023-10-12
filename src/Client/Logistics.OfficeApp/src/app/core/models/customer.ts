import {Invoice} from './invoice';

export interface Customer {
  name: string;
  invoices: Invoice[];
}
