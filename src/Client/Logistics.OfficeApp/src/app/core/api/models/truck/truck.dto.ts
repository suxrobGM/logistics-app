import {AddressDto} from "../address.dto";
import {EmployeeDto} from "../employee/employee.model";
import {GeoPointDto} from "../geo-point.dto";
import {LoadDto} from "../load/load.dto";
import {TruckStatus, TruckType} from "./enums";

export interface TruckDto {
  id: string;
  number: string;
  type: TruckType;
  status: TruckStatus;
  mainDriver?: EmployeeDto;
  secondaryDriver?: EmployeeDto;
  currentAddress?: AddressDto;
  currentLocation?: GeoPointDto;
  loads?: LoadDto[];
}
