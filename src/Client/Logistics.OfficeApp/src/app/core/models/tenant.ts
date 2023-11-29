import {Address} from './address';

export interface Tenant {
  id: string;
  name: string;
  companyName: string;
  companyAddress: Address;
}
