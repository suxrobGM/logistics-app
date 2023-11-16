import {Address} from '../address';
import {Employee} from '../employee/employee';
import {Load} from '../load/load';

export interface Truck {
  id: string;
  truckNumber: string;
  drivers: Employee[];
  currentLocation?: Address;
  currentLocationLat?: number;
  currentLocationLong?: number;
  loads?: Load[];
}
