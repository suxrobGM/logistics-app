import {EmployeeDto} from "../employee/employee.dto";
import {TruckDto} from "./truck.dto";

export interface TruckDriverDto {
  truck: TruckDto;
  drivers: EmployeeDto[];
}
