import {LoadStatus} from '@core/enums';
import {Address} from '../address';

export interface UpdateLoad {
  id: string;
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
  customerId?: string;
  status?: LoadStatus;
}
