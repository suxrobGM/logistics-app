import {LoadStatus} from './loadStatus';
import {Truck} from './truck';

export interface Load {
  id: string;
  refId: number;
  name?: string;
  originAddress: string;
  originAddressLat: number;
  originAddressLong: number;
  destinationAddress: string;
  destinationAddressLat: number;
  destinationAddressLong: number;
  deliveryCost: number;
  distance: number;
  status: LoadStatus;
  dispatchedDate: string;
  pickUpDate: string;
  deliveryDate: string;
  assignedDispatcherId: string;
  assignedDispatcherName: string;
  assignedTruck: Truck;
}
