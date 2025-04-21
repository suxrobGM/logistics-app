import {AddressDto} from "../address.model";
import {EmployeeDto} from "../employee/employee.model";
import {LoadDto} from "../load/load.model";

export interface TruckDto {
  id: string;
  truckNumber: string;
  drivers: EmployeeDto[];
  currentLocation?: AddressDto;
  currentLocationLat?: number;
  currentLocationLong?: number;
  loads?: LoadDto[];
}
