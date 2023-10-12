import {Customer} from './customer';
import {Invoice} from './invoice';
import {LoadStatus} from './loadStatus';


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
  assignedDispatcherName?: string;
  assignedTruckId: string;
  assignedTruckNumber?: string;
  assignedTruckDriversName?: string[];
  currentLocation?: string;
  customer?: Customer;
  invoice?: Invoice;
}
