import {Address} from '../Address_';

export interface CreateLoad {
  name?: string;
  originAddress: Address;
  originAddressLat: number;
  originAddressLong: number;
  destinationAddress: Address;
  destinationAddressLat: number;
  destinationAddressLong: number;
  deliveryCost: number;
  distance: number;
  assignedDispatcherId: string;
  assignedTruckId: string;
  customerId: string;
}
