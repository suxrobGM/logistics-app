import {AddressDto} from "../address.dto";
import {GeoPointDto} from "../geo-point.dto";
import {LoadType} from "./enums";

export interface CreateLoadCommand {
  name: string;
  loadType: LoadType;
  originAddress: AddressDto;
  originLocation: GeoPointDto;
  destinationAddress: AddressDto;
  destinationLocation: GeoPointDto;
  deliveryCost: number;
  distance: number;
  assignedDispatcherId: string;
  assignedTruckId: string;
  customerId?: string;
}
