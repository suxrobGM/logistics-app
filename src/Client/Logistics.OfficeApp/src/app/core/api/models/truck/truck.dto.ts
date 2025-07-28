import {AddressDto} from "../address.dto";
import {EmployeeDto} from "../employee/employee.model";
import {LoadDto} from "../load/load.dto";
import {TruckStatus, TruckType} from "./enums";

export interface TruckDto {
  id: string;
  number: string;
  type: TruckType;
  status: TruckStatus;
  mainDriver?: EmployeeDto;
  secondaryDriver?: EmployeeDto;
  currentLocation?: AddressDto;
  currentLocationLat?: number;
  currentLocationLong?: number;
  loads?: LoadDto[];
}
