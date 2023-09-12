import {Employee} from './employee';

export interface Truck {
  id: string;
  truckNumber: string;
  drivers: Employee[];
  loadIds?: string[];
}
