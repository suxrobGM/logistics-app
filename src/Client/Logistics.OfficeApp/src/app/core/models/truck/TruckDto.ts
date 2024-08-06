import {AddressDto} from '../AddressDto';
import {EmployeeDto} from '../employee/EmployeeDto';
import {LoadDto} from '../load/LoadDto';

export interface TruckDto {
  id: string;
  truckNumber: string;
  drivers: EmployeeDto[];
  currentLocation?: AddressDto;
  currentLocationLat?: number;
  currentLocationLong?: number;
  loads?: LoadDto[];
}
