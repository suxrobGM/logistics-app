import {Employee} from './employee';

export interface TruckStats {
  truckId: string;
  truckNumber: string;
  startDate: string;
  endDate: string;
  gross: number;
  distance: number;
  drivers: Employee[];
}
