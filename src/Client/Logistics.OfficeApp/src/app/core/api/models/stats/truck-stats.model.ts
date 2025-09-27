import { EmployeeDto } from "../employee/employee.model";

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
