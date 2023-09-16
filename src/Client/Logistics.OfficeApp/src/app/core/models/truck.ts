import {Employee} from './employee';

export interface Truck {
  id: string;
  truckNumber: string;
  drivers: Employee[];
  currentLocation?: string;
  currentLocationLat?: number;
  currentLocationLong?: number;
  loadIds?: string[];
}
