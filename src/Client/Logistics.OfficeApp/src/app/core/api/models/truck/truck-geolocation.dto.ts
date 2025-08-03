import {AddressDto} from "../address.dto";
import {GeoPointDto} from "../geo-point.dto";

export interface TruckGeolocationDto {
  truckId: string;
  tenantId?: string;
  currentLocation?: GeoPointDto;
  currentAddress?: AddressDto;
  truckNumber?: string;
  driversName?: string;
}
