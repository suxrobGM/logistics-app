import {AddressDto} from "../address.dto";
import {LoadType} from "./enums";

export interface CreateLoadCommand {
  name: string;
  loadType: LoadType;
  originAddress: AddressDto;
  originAddressLat: number;
  originAddressLong: number;
  destinationAddress: AddressDto;
  destinationAddressLat: number;
  destinationAddressLong: number;
  deliveryCost: number;
  distance: number;
  assignedDispatcherId: string;
  assignedTruckId: string;
  customerId: string;
}
