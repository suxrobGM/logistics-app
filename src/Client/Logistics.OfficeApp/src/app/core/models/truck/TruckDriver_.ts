import {Employee} from '../employee/employee';
import {Truck} from './Truck_';

export interface TruckDriver {
  truck: Truck,
  drivers: Employee[]
}
