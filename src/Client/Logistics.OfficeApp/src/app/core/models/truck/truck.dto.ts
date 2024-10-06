import {AddressDto} from "../AddressDto";
import {EmployeeDto} from "../employee/employee.dto";
import {LoadDto} from "../load/load.dto";

export interface TruckDto {
  id: string;
  truckNumber: string;
  drivers: EmployeeDto[];
  currentLocation?: AddressDto;
  currentLocationLat?: number;
  currentLocationLong?: number;
  loads?: LoadDto[];
}
