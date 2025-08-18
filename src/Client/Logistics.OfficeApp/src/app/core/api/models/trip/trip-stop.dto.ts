import {AddressDto} from "../address.dto";
import {GeoPointDto} from "../geo-point.dto";
import {TripStopType} from "./enums";

export interface TripStopDto {
  id: string;
  order: number;
  type: TripStopType;
  address: AddressDto;
  location: GeoPointDto;
  arrivedAt?: Date;
  loadId: string;
}
