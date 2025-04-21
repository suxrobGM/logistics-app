import {EmployeeDto} from "../employee/employee.model";
import {TruckDto} from "./truck.dto";

export interface TruckDriverDto {
  truck: TruckDto;
  drivers: EmployeeDto[];
}
