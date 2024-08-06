import {Address} from './Address';

export interface Tenant {
  id: string;
  name: string;
  companyName: string;
  companyAddress: Address;
}
