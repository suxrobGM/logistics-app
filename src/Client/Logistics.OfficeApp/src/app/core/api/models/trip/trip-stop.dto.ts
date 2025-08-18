import {AddressDto} from "../address.dto";
import {TripStopType} from "./enums";

export interface TripStopDto {
  id: string;
  order: number;
  type: TripStopType;
  address: AddressDto;
  addressLong: number;
  addressLat: number;
  arrivedAt?: Date;
  loadId: string;
}
