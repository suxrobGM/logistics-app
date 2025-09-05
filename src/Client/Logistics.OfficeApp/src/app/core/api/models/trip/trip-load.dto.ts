import {AddressDto} from "../address.dto";
import {CustomerDto} from "../customer";
import {GeoPointDto} from "../geo-point.dto";
import {LoadStatus, LoadType} from "../load";

export interface TripLoadDto {
  id: string;
  number: number;
  name: string;
  status: LoadStatus;
  type: LoadType;
  distance: number;
  deliveryCost: number;
  originAddress: AddressDto;
  originLocation: GeoPointDto;
  destinationAddress: AddressDto;
  destinationLocation: GeoPointDto;
  customer?: CustomerDto | null;
}
