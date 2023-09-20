import {Employee} from './employee';
import {Load} from './load';

export interface Truck {
  id: string;
  truckNumber: string;
  drivers: Employee[];
  currentLocation?: string;
  currentLocationLat?: number;
  currentLocationLong?: number;
  loads?: Load[];
}
