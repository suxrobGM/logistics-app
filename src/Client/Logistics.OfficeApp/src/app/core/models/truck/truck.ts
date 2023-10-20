import {Employee} from '../employee/employee';
import {Load} from '../load/load';

export interface Truck {
  id: string;
  truckNumber: string;
  drivers: Employee[];
  currentLocation?: string;
  currentLocationLat?: number;
  currentLocationLong?: number;
  loads?: Load[];
}
