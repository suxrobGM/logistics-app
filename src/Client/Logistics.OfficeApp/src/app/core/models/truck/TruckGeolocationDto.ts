import {AddressDto} from "../AddressDto";

export interface TruckGeolocationDto {
  truckId: string;
  tenantId?: string;
  latitude: number;
  longitude: number;
  currentAddress?: AddressDto;
  truckNumber?: string;
  driversName?: string;
}
