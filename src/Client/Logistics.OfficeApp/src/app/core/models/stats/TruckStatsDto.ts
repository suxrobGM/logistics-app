import {EmployeeDto} from '../employee/EmployeeDto';

export interface TruckStatsDto {
  truckId: string;
  truckNumber: string;
  startDate: string;
  endDate: string;
  gross: number;
  distance: number;
  driverShare: number;
  drivers: EmployeeDto[];
}
