import {Employee} from './employee';
import {Truck} from './truck';

export interface TruckDriver {
  truck: Truck,
  drivers: Employee[]
}
