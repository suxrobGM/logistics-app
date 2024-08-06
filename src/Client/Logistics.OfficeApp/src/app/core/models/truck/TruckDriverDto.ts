import {EmployeeDto} from '../employee/EmployeeDto';
import {TruckDto} from './TruckDto';

export interface TruckDriverDto {
  truck: TruckDto,
  drivers: EmployeeDto[]
}
