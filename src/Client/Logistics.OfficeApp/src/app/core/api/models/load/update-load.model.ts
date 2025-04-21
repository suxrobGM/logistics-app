import {LoadStatus} from "@/core/enums";
import {AddressDto} from "../address.model";

export interface UpdateLoadCommand {
  id: string;
  name?: string;
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
  customerId?: string;
  status?: LoadStatus;
}
