import {LoadStatus} from '@core/enums';
import {Customer} from '../customer';
import {Invoice} from '../invoice';
import {Address} from '../Address_';


export interface Load {
  id: string;
  refId: number;
  name?: string;
  originAddress: Address;
  originAddressLat: number;
  originAddressLong: number;
  destinationAddress: Address;
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
